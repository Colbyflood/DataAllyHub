// See https://aka.ms/new-console-template for more information

using FacebookLoader.Common;
using FacebookLoader.Loader.AdCreative;
using FacebookLoader.Loader.AdImage;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Facebook Loader Testbed");

var builder = new ConfigurationBuilder()
	.AddUserSecrets<Program>(); // Use the assembly containing the UserSecretsId

IConfiguration configuration = builder.Build();

var token = configuration["token"];
var channelAccount = configuration["channelAccount"];

var logger = new Logging();

var facebookParameters = new FacebookParameters(channelAccount, token);

try
{
	// var adImagesLoader = new AdImagesLoader(facebookParameters, logger);
	//
	// var response = await adImagesLoader.StartLoad(true);
	// foreach (var content in response.Content)
	// {
	// 	Console.WriteLine(content.Name);
	// }

	var adCreativeLoader = new AdCreativesLoader(facebookParameters, logger);

	var response = await adCreativeLoader.StartLoad(true);
	if (response == null)
	{
		Console.WriteLine("Failed to load ad creatives");
	}
	else
	{
		foreach (var content in response.Content)
		{
			Console.WriteLine(content.Name);
		}
	}
}
catch (Exception ex)
{
	PrintExceptionDetails(ex);
}


static void PrintExceptionDetails(Exception ex)
{
	Console.WriteLine("Exception Details:");
	Console.WriteLine($"Type: {ex.GetType().FullName}");
	Console.WriteLine($"Message: {ex.Message}");
	Console.WriteLine($"Stack Trace: {ex.StackTrace}");

	// Print inner exceptions recursively
	if (ex.InnerException != null)
	{
		Console.WriteLine("Inner Exception:");
		PrintExceptionDetails(ex.InnerException);
	}
}


public class Logging : ILogging
{
	public void LogError(string message)
	{
		Console.WriteLine($"LOGGING ERROR: {message}");
	}

	public void LogWarning(string message)
	{
		Console.WriteLine($"LOGGING WARNING: {message}");
	}

	public void LogInformation(string message)
	{
		Console.WriteLine($"LOGGING INFO: {message}");
	}

	public void LogDebug(string message)
	{
		Console.WriteLine($"LOGGING DEBUG: {message}");
	}
}