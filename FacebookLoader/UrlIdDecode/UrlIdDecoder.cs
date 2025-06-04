using System.Net.Http.Headers;
using FacebookLoader.Common;
using FacebookLoader.Content;
using Newtonsoft.Json;

namespace FacebookLoader.UrlIdDecode;

public class UrlIdDecoder
{
	// ReSharper disable once InconsistentNaming
	private const int SOCKET_TIMEOUT_SECONDS = 30;
	
	public static async Task<List<FacebookImageUrl>> DecodeImageHash(FacebookParameters facebookParameters, string imageHash)
	{
		const string fieldsList = "url";
			
		using var httpClient = new HttpClient()
		{
			Timeout = TimeSpan.FromSeconds(SOCKET_TIMEOUT_SECONDS)
		};
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		HttpResponseMessage? response = null;
		try
		{
			string url = $"{facebookParameters.CreateUrlFor("adimages")}?fields={fieldsList}&hashes[]={imageHash}&access_token={facebookParameters.Token}";

			response = await httpClient.GetAsync(url);
			response.EnsureSuccessStatusCode();

			var responseContent = await response.Content.ReadAsStringAsync();
			var tokenResponse = JsonConvert.DeserializeObject<FacebookAdImagesResponse>(responseContent);
			return tokenResponse != null ? tokenResponse.Data : new List<FacebookImageUrl>();
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

	public static async Task<FacebookVideoSource?> DecodeVideoId(FacebookParameters facebookParameters, string pageToken, string videoId)
	{
		const string fieldsList = "source";
			
		using var httpClient = new HttpClient()
		{
			Timeout = TimeSpan.FromSeconds(SOCKET_TIMEOUT_SECONDS)
		};
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		HttpResponseMessage? response = null;
		try
		{
			string url = $"{facebookParameters.GetBaseUrl()}/{videoId}/accounts?fields={fieldsList}&access_token={pageToken}";

			response = await httpClient.GetAsync(url);
			response.EnsureSuccessStatusCode();

			var responseContent = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<FacebookVideoSource>(responseContent);
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
	

	public record FacebookAdImagesResponse(List<FacebookImageUrl> Data);
}

