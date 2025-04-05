using System.Net.Http.Headers;
using System.Text.Json;
using FacebookLoader.Common;

namespace FacebookLoader.Loader;


public abstract class FacebookLoaderBase
{
    public FacebookParameters FacebookParameters { get; }

    public FacebookLoaderBase(FacebookParameters facebookParameters) => this.FacebookParameters = facebookParameters;

    protected async Task<JsonDocument> CallGraphApiAsync(string url)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonDocument.ParseAsync(stream);
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

    public static string ExtractString(JsonElement obj, string tag)
    {
        if (!obj.TryGetProperty(tag, out var value)) return string.Empty;
        return value.GetString() ?? string.Empty;
    }

    public static bool ExtractBoolean(JsonElement obj, string tag)
    {
        if (!obj.TryGetProperty(tag, out var value)) return false;
        return value.GetBoolean();
    }

    public static JsonElement? ExtractObject(JsonElement obj, string tag)
    {
        if (!obj.TryGetProperty(tag, out var value)) return null;
        return value;
    }

    public static List<JsonElement> ExtractObjectArray(JsonElement obj, string tag)
    {
        if (!obj.TryGetProperty(tag, out var arrayElement) || arrayElement.ValueKind != JsonValueKind.Array)
            return new List<JsonElement>();

        var list = new List<JsonElement>();
        foreach (var element in arrayElement.EnumerateArray())
        {
            list.Add(element);
        }
        return list;
    }
}
