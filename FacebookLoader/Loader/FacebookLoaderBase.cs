using System.Net.Http.Headers;
using System.Text;
using FacebookLoader.Common;
using Newtonsoft.Json.Linq;

namespace FacebookLoader.Loader;


public abstract class FacebookLoaderBase
{
    // ReSharper disable once InconsistentNaming
    private const int SOCKET_TIMEOUT_SECONDS = 120;

    public FacebookParameters FacebookParameters { get; }
    public ILogging Logger { get; }

    public FacebookLoaderBase(FacebookParameters facebookParameters, ILogging logger)
    {
        this.FacebookParameters = facebookParameters;
        this.Logger = logger;
    }

    protected static string GetSanitizedUrl(string url)
    {
        var questionIndex = url.IndexOf("?");
        if (questionIndex < 0)
        {
            return url;     // no parameters means no access_token is in url
        }

        var urlBase = url.Substring(0, questionIndex);

        var parameters = url.Substring(questionIndex + 1).Split('&');
        var sanitizedUrl = new StringBuilder();
        sanitizedUrl.Append(urlBase);
        sanitizedUrl.Append("?");
        var paramCount = 0;
        foreach (var parameter in parameters)
        {
            var equalsIndex = parameter.IndexOf('=');
            if (equalsIndex < 0)
            {
                if (paramCount > 0)
                {
                    sanitizedUrl.Append("&");
                }
                sanitizedUrl.Append(parameter);
                ++paramCount;
            }
            else
            {
                var key = parameter.Substring(0, equalsIndex).ToLower();
                if (key != "access_token")
                {
                    if (paramCount > 0)
                    {
                        sanitizedUrl.Append("&");
                    }
                    sanitizedUrl.Append(parameter);
                    ++paramCount;
                }
            }

        }

        return sanitizedUrl.ToString();
    }

    protected static int? GetLimitFromUrl(string url)
    {
        var questionIndex = url.IndexOf("?");
        if (questionIndex < 0)
        {
            return null;     // no parameters means no access_token is in url
        }

        var urlBase = url.Substring(0, questionIndex);

        var parameters = url.Substring(questionIndex + 1).Split('&');
        var updatedUrl = new StringBuilder();
        updatedUrl.Append(urlBase);
        updatedUrl.Append("?");
        foreach (var parameter in parameters)
        {
            var equalsIndex = parameter.IndexOf('=');
            if (equalsIndex > 0)
            {
                var key = parameter.Substring(0, equalsIndex).ToLower();
                if (key == "limit")
                {
                    var value = parameter.Substring(equalsIndex + 1).Trim();
                    if (int.TryParse(value, out var result))
                    {
                        return result;
                    }
                }
            }
        }

        return null;
    }

    protected static string UpdateUrlWithLimit(string url, int limit)
    {
        var questionIndex = url.IndexOf("?");
        if (questionIndex < 0)
        {
            return url;     // no parameters means no access_token is in url
        }

        var urlBase = url.Substring(0, questionIndex);

        var parameters = url.Substring(questionIndex + 1).Split('&');
        var updatedUrl = new StringBuilder();
        updatedUrl.Append(urlBase);
        updatedUrl.Append("?");
        var foundLimit = false;
        var paramCount = 0;
        foreach (var parameter in parameters)
        {
            var equalsIndex = parameter.IndexOf('=');
            var value = parameter;
            if (equalsIndex > 0)
            {
                var key = parameter.Substring(0, equalsIndex).ToLower();
                if (key == "limit")
                {
                    value = $"limit={limit}";
                    foundLimit = true;
                }
            }

            if (paramCount > 0)
            {
                updatedUrl.Append("&");
            }
            updatedUrl.Append(value);
            ++paramCount;
        }

        if (!foundLimit)
        {
            if (paramCount > 0)
            {
                updatedUrl.Append("&");
            }
            updatedUrl.Append($"limit={limit}");
        }

        return updatedUrl.ToString();
    }

    protected async Task<JObject> CallGraphApiAsync(string url)
    {
        using var httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(SOCKET_TIMEOUT_SECONDS)
        };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage? response = null;
        try
        {
            response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseContent);
        }
        catch (HttpRequestException httpEx) when (httpEx.StatusCode.HasValue)
        {
            //var responseText = httpEx.Message;
            var responseText = string.Empty;

            if (response != null)
            {
                responseText = await response.Content.ReadAsStringAsync();
            }

            Console.Error.WriteLine($"HTTP error occurred: {httpEx} while calling graph api");
            throw new FacebookHttpException((int)httpEx.StatusCode.Value, responseText);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Error while copying content to a stream",StringComparison.Ordinal)) // Incase stream was closed by host
            {
                throw new FacebookHttpException(-1, $"FacebookLoaderBase:CallGraphApiAsync : {ex.Message} : {ex.InnerException?.Message}");
            }

            Console.Error.WriteLine($"An error occurred: {ex} while calling graph api");
            throw new Exception($"FacebookLoaderBase:CallGraphApiAsync Other error occurred: {ex.Message} : {ex.InnerException?.Message}");
        }
    }

    public static string ExtractString(JToken? obj, string tag)
    {
        JToken? value = obj?[tag];
        if (value == null)
        {
            return string.Empty;
        }
        return value.ToString();
    }

    public static bool ExtractBoolean(JToken? obj, string tag)
    {
        JToken? value = obj?[tag];
        if (value == null)
        {
            return false;
        }
        return value.ToObject<bool>();
    }

    public static JToken? ExtractObject(JToken? obj, string tag)
    {
        if (obj == null)
        {
            return null;
        }

        if (obj.Type == JTokenType.Array)
        {
            return obj;
        }

        if (obj.Type == JTokenType.Object)
        {
            var jObject = (JObject)obj;
            return jObject.TryGetValue(tag, out JToken value) ? value : null;
        }

        return null;
    }

    public static List<JObject> ExtractObjectArray(JToken? obj, string tag)
    {
        JToken? value = obj?[tag];
        if (value is not JArray arrayElement)
        {
            return new List<JObject>();
        }
        var list = new List<JObject>();
        foreach (var element in arrayElement)
        {
            if (element is JObject jObject)
            {
                list.Add(jObject);
            }
        }
        return list;
    }
}
