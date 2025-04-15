using System.Net.Http.Headers;
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

    protected async Task<JObject> CallGraphApiAsync(string url)
    {
        using var httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(SOCKET_TIMEOUT_SECONDS)
        };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseContent);
        }
        catch (HttpRequestException httpEx) when (httpEx.StatusCode.HasValue)
        {
            var responseText = httpEx.Message;
            Console.Error.WriteLine($"HTTP error occurred: {httpEx} while calling graph api");
            throw new FacebookHttpException((int)httpEx.StatusCode.Value, responseText);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred: {ex} while calling graph api");
            throw new Exception($"Other error occurred: {ex.Message}");
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
