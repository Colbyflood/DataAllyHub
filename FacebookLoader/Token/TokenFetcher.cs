using System.Net.Http.Headers;
using FacebookLoader.Common;
using FacebookLoader.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FacebookLoader.Token;

public class TokenFetcher
{
	// ReSharper disable once InconsistentNaming
	private const int SOCKET_TIMEOUT_SECONDS = 30;
	
	public static async Task<FacebookAccount?> GetFacebookAccountForToken(FacebookParameters facebookParameters, string token)
	{
		const string FieldsList = "id,name";
			
		using var httpClient = new HttpClient()
		{
			Timeout = TimeSpan.FromSeconds(SOCKET_TIMEOUT_SECONDS)
		};
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		HttpResponseMessage? response = null;
		try
		{
			string url = $"{facebookParameters.CreateUrlFor("ads")}?fields={FieldsList}&access_token={token}";

			response = await httpClient.GetAsync(url);
			response.EnsureSuccessStatusCode();

			var responseContent = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<FacebookAccount>(responseContent);
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
			Console.Error.WriteLine($"An error occurred: {ex} while calling graph api");
			throw new Exception($"Other error occurred: {ex.Message}");
		}
	}
	
	public static async Task<List<FacebookPageToken>> GetFacebookPageTokensForAccount(FacebookParameters facebookParameters, string accountNumber, string token)
	{
		const string FieldsList = "id,name,access_token";

		using var httpClient = new HttpClient()
		{
			Timeout = TimeSpan.FromSeconds(SOCKET_TIMEOUT_SECONDS)
		};
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		HttpResponseMessage? response = null;
		try
		{
			string url = $"{facebookParameters.CreateUrlFor("ads")}?fields={FieldsList}&access_token={token}";

			response = await httpClient.GetAsync(url);
			response.EnsureSuccessStatusCode();

			var responseContent = await response.Content.ReadAsStringAsync();
			var tokenResponse = JsonConvert.DeserializeObject<FacebookPageTokenResponse>(responseContent);
			return tokenResponse != null ? tokenResponse.Data : new List<FacebookPageToken>();
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
			Console.Error.WriteLine($"An error occurred: {ex} while calling graph api");
			throw new Exception($"Other error occurred: {ex.Message}");
		}
	}
	
	public record FacebookPageTokenResponse(List<FacebookPageToken> Data);
}