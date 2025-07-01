using Newtonsoft.Json;

namespace FacebookLoader.Content;

public class FacebookAdCreativesResponse
{
    public List<FacebookAdCreative> Content { get; }
    public bool IsSuccessful { get; }
    public string? RestartUrl { get; }
    public bool NotPermitted { get; }
    public bool TokenExpired { get; }
    public bool Throttled { get; }
    public bool TemporaryDowntime { get; }
    public string ExceptionBody { get; }

    [JsonConstructor]
    public FacebookAdCreativesResponse(
        List<FacebookAdCreative> content,
        bool isSuccessful = true,
        string? restartUrl = null,
        bool notPermitted = false,
        bool tokenExpired = false,
        bool throttled = false,
        bool temporaryDowntime = false,
        string exceptionBody = ""
        )
    {
        Content = content;
        IsSuccessful = isSuccessful;
        RestartUrl = restartUrl;
        NotPermitted = notPermitted;
        TokenExpired = tokenExpired;
        Throttled = throttled;
        TemporaryDowntime = temporaryDowntime;
        ExceptionBody = exceptionBody;
    }

    public static FacebookAdCreativesResponse? FromJson(string json)
    {
        return JsonConvert.DeserializeObject<FacebookAdCreativesResponse>(json);
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }
}
