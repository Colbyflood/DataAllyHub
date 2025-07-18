// See https://aka.ms/new-console-template for more information

using FacebookLoader.Common;
using FacebookLoader.Content;
using FacebookLoader.Loader.AdCreative;
using FacebookLoader.Loader.AdImage;
using FacebookLoader.Loader.AdInsight;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Facebook Loader Testbed");

var builder = new ConfigurationBuilder()
    .AddUserSecrets<Program>(); // Use the assembly containing the UserSecretsId

IConfiguration configuration = builder.Build();

// Ua
var token = "EAAD7wZCNwaOgBPFwBSMsF0qGkZB6Is5SGz2m56DfbPrnVF4ZCFaB6mZCFDmxp9VzVGDaOoSgorxtZAkTZB0tn1ZAJfe8xFLDjsjFsCLa1OEyr8OegQHjGSnqzpe1VQuFjYQPzHtm5u30cJ3CB4iwhQ3ZAf9PZBdmcnNsANQadWti5z4JHj8NMpJjLaE8CxMTrQffT";
var channelAccount = "act_239631127116758";

var logging = new Logging();

var facebookParameters = new FacebookParameters(channelAccount, token);

try
{
    // Addcretive code
    var loader = new AdCreativesLoader(facebookParameters, logging);
    var response = await loader.StartLoadAsync();

    if (response!=null)
    {
        var content = response.ToJson();
    }

    // AdInsights Test Code
    //var now = DateTime.UtcNow;
    //var windowStartTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
    //var yesterday = windowStartTime.AddDays(-3);
    //var startDay = windowStartTime.AddDays(-3); // Fetch data for last 1 days

    //var customDate = new DateTime(2025, 7, 4);

    //var loader = new AdInsightsLoader(facebookParameters, logging);
    //var response = await loader.StartLoadAsync(null, null);

    //if (response == null)
    //{

    //}

    //if (response.Content.Count > 0)
    //{
    //    int contentCount = response.Content.Count;

    //    var content = response.ToJson();

    //    var x = "dd";

    //    //int batchSize = MAX_FB_STAGING_RECORDS_IN_ROWS;

    //    //int totalBatches = (contentCount + batchSize - 1) / batchSize;

    //    //for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
    //    //{
    //    //    var batch = response.Content.Skip(batchIndex * batchSize).Take(batchSize).ToList<FacebookAdInsight>();

    //    //    var content = new FacebookAdInsightsResponse(batch);

    //    //    var runStaging = new FbRunStaging();
    //    //    runStaging.FbRunlogId = runlog.Id;
    //    //    runStaging.Sequence = GetNextSequence(runlog);
    //    //    runStaging.Content = content.ToJson();

    //    //    loaderProxy.WriteFbRunStaging(runStaging);
    //    //}
    ////}

    //var adImagesLoader = new AdImagesLoader(facebookParameters, logging);
    //var response = await adImagesLoader.StartLoadAsync();

    // var adImagesLoader = new AdImagesLoader(facebookParameters, logger);
    //
    // var response = await adImagesLoader.StartLoadAsync(true);
    // foreach (var content in response.Content)
    // {
    // 	Console.WriteLine(content.Name);
    // }
    // Console.WriteLine("Converting to JSON");
    // var json = response.ToJson();
    // Console.WriteLine(json);
    // 	
    // Console.WriteLine("Rehydrating objects");
    // var newContent = FacebookAdImagesResponse.FromJson(json);
    // foreach (var content in newContent.Content)
    // {
    // 	Console.WriteLine($"NOW: {content.Name}");
    // }

    // var adCreativeLoader = new AdCreativesLoader(facebookParameters, logger);
    //
    // var response = await adCreativeLoader.StartLoadAsync(true);
    // if (response == null)
    // {
    // 	Console.WriteLine("Failed to load ad creatives");
    // }
    // else
    // {
    // 	foreach (var content in response.Content)
    // 	{
    // 		Console.WriteLine(content.Name);
    // 	}
    //
    // 	Console.WriteLine("Converting to JSON");
    // 	var json = response.ToJson();
    // 	Console.WriteLine(json);
    // 	
    // 	Console.WriteLine("Rehydrating objects");
    // 	var newContent = FacebookAdCreativesResponse.FromJson(json);
    // 	foreach (var content in newContent.Content)
    // 	{
    // 		Console.WriteLine($"NOW: {content.Name}");
    // 	}
    // }

    // var adInsightsLoader = new AdInsightsLoader(facebookParameters, logger);
    //
    // var response = await adInsightsLoader.StartLoadAsync("2025-04-03", "2025-04-07", true);
    // if (response == null)
    // {
    // 	Console.WriteLine("Failed to load ad creatives");
    // }
    // else
    // {
    // 	foreach (var content in response.Content)
    // 	{
    // 		Console.WriteLine(content.Name);
    // 	}
    // 	
    // 	Console.WriteLine("Converting to JSON");
    // 	var json = response.ToJson();
    // 	Console.WriteLine(json);
    // 	
    // 	Console.WriteLine("Rehydrating objects");
    // 	var newContent = FacebookAdInsightsResponse.FromJson(json);
    // 	foreach (var content in newContent.Content)
    // 	{
    // 		Console.WriteLine($"NOW: {content.Name}");
    // 	}
    // }

    var s =
        "{\n    \"data\": [\n        {\n            \"account_id\": \"239631127116758\",\n            \"id\": \"120227850606640613\",\n            \"name\": \"2605 // CR105 | Gif | Best Sellers | Features\",\n            \"status\": \"ACTIVE\",\n            \"adset_id\": \"120227848865770613\",\n            \"campaign_id\": \"120224387399220613\",\n            \"created_time\": \"2025-05-26T10:35:40-1000\",\n            \"updated_time\": \"2025-05-26T11:01:05-1000\",\n            \"creative\": {\n                \"id\": \"1146720267140135\",\n                \"status\": \"ACTIVE\",\n                \"actor_id\": \"108773893960546\",\n                \"video_id\": \"730700836046595\",\n                \"instagram_actor_id\": \"2731436270247461\",\n                \"instagram_permalink_url\": \"https://www.instagram.com/p/DKIWKkfs1BM/\",\n                \"object_type\": \"VIDEO\",\n                \"thumbnail_url\": \"https://scontent-atl3-2.xx.fbcdn.net/v/t15.5256-10/501986446_1424472678565081_4426632814957002795_n.jpg?_nc_cat=111&ccb=1-7&_nc_ohc=rhjRryL3GMEQ7kNvwE8fCIE&_nc_oc=AdldrluSudnZEUXUi4uMV7eYHWxvF3_sCq1nUVTkld7eoZRgXKr2KA1bt9Nl5QXoGfk&_nc_zt=23&_nc_ht=scontent-atl3-2.xx&edm=AOgd6ZUEAAAA&_nc_gid=BlsUS1eiHF51itAg5x-dcQ&stp=c0.5000x0.5000f_dst-emg0_p64x64_q75_tt6&ur=282d23&_nc_sid=58080a&oh=00_AfIuXv9e8PsbiLNumFDFjCgJSuTy8VQ8Vay7aMiNTRyqDw&oe=684628AD\",\n                \"thumbnail_id\": \"1412368256482324\",\n                \"title\": \"Shop our best-sellers\",\n                \"body\": \"Embrace the Aloha spirit with our collection of most-loved scents and products. \ud83c\udf3a\\n\\n\\\"I've been wearing it for 18 or so years and get compliments on how I smell every single day.\\\" - Sally W.\\n\\nOur natural, clean, vegan products will transport you to the parts of Hawaiʻi you love most, from the beaches and coastlines to the rainforests and tropical canyons.\",\n                \"object_story_spec\": {\n                    \"page_id\": \"108773893960546\",\n                    \"video_data\": {\n                        \"video_id\": \"1007135618158178\",\n                        \"title\": \"Shop our best-sellers\",\n                        \"message\": \"Embrace the Aloha spirit with our collection of most-loved scents and products. \ud83c\udf3a\\n\\n\\\"I've been wearing it for 18 or so years and get compliments on how I smell every single day.\\\" - Sally W.\\n\\nOur natural, clean, vegan products will transport you to the parts of Hawaiʻi you love most, from the beaches and coastlines to the rainforests and tropical canyons.\",\n                        \"call_to_action\": {\n                            \"type\": \"SHOP_NOW\",\n                            \"value\": {\n                                \"link\": \"https://uabody.com/en-au/pages/best-sellers\"\n                            }\n                        },\n                        \"image_url\": \"https://www.facebook.com/ads/image/?d=AQK7jr_t3mh_4UHQFUpGvWt9cNJ8bCZYcsWJdI6ud-2GLFMh9GrhjiAK4IVz1o04Y7sEW5jVFIrAHcHg6WwTYT4-KKuo6PdZKc-06gcI69ncrEjZYRSW8-9vkBqTyIF7SGN5f1T636tpazoF5ELBxSIm\",\n                        \"image_hash\": \"ef66e9af8d786acc2afae0aa5c2e175e\"\n                    }\n                }\n            }\n        }\n   ],\n    \"paging\": {\n        \"cursors\": {\n            \"before\": \"QVFIUnl2WFNHWXc0YnU1Uk1fNWFDQlVKZA01GOXZAFQjVYQjFEU3l4N1l6TVI0SFc4SlJEVXZAiTS1Tcl9BY1dHcWlOMTUZD\",\n            \"after\": \"QVFIUkFtWDdMdzBBRHdzeTlvM0licWR3VUlTQUxuT3dlOE1STHJXTFlreGljMkh1NnVDclBHNTlVWEg1UndYSUFUQ0EZD\"\n        },\n        \"next\": \"https://graph.facebook.com/v21.0/act_239631127116758/ads?fields=account_id,id,name,status,adset_id,campaign_id,created_time,updated_time,creative{id,status,actor_id,video_id,instagram_actor_id,instagram_permalink_url,object_type,image_url,image_hash,thumbnail_url,thumbnail_id,product_set_id,url_tags,title,body,link_destination_display_url,product_data,template_url_spec,template_url,object_story_spec{page_id,link_data,video_data,template_data}}&limit=50&access_token=EAAD7wZCNwaOgBOzzOuFME3OQE6BJkj70HNZAx5KY8GZBMowUAcMybf8poRKzXYpjzjNm7IRa3r6EyyV0MdnZAgKp4R2pex9tG8BdbQhrv8lKQBnZAkGxXQXKy5N5gV9HA4kgFknrYZArn9QiaiP9YnTBoMcUYUQsNeZBmenpWKZCsH59LzgAuCbr0HHGr0p3&after=QVFIUkFtWDdMdzBBRHdzeTlvM0licWR3VUlTQUxuT3dlOE1STHJXTFlreGljMkh1NnVDclBHNTlVWEg1UndYSUFUQ0EZD\"\n    } }";

    var value = AdCreativesLoader.DigestJsonStringItem(s);
    Console.WriteLine("Done");
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

    public void LogException(Exception ex, string message)
    {
        Console.WriteLine($"LOGGING EXCEPTION: {message}");
        PrintExceptionDetails(ex);
    }

    private static void PrintExceptionDetails(Exception ex)
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
}