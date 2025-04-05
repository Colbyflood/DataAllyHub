using System.Text.Json;
using System.Text.Json.Nodes;

namespace FacebookLoader.Common;

public class FacebookHttpException : Exception
{
    public int HttpCode { get; }
    public string TypeCode { get; private set; } = string.Empty;
    public string ErrorCode { get; private set; } = string.Empty;
    public string ErrorSubcode { get; private set; } = string.Empty;

    public bool TokenExpired { get; private set; } = false;
    public bool Throttled { get; private set; } = false;
    public bool NotPermitted { get; private set; } = false;

    public FacebookHttpException(int httpCode, string errorData)
    {
        HttpCode = httpCode;

        try
        {
            var root = JsonNode.Parse(errorData);
            var error = root?["error"];

            if (error == null)
                throw new JsonException("Missing 'error' node in JSON");

            Message = error?["message"]?.ToString() ?? "Unknown Facebook error";
            TypeCode = error?["type"]?.ToString() ?? string.Empty;
            ErrorCode = error?["code"]?.ToString() ?? string.Empty;
            ErrorSubcode = error?["error_subcode"]?.ToString() ?? string.Empty;

            if (TypeCode == "OAuthException")
            {
                if (ErrorCode == "4" && ErrorSubcode == "1504022")
                    Throttled = true;
                else
                    TokenExpired = true;
            }
            else
            {
                if (string.Compare(ErrorCode, "190") < 0)
                    TokenExpired = true;
                else if (string.Compare(ErrorCode, "200") >= 0 && string.Compare(ErrorCode, "300") < 0)
                    NotPermitted = true;
                else if (ErrorCode == "3" || ErrorCode == "10" || ErrorCode == "368")
                    NotPermitted = true;
                else
                    Throttled = true;
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"Cannot parse http exception data: {errorData}");
            NotPermitted = true;
        }
    }

    public override string Message { get; }
}