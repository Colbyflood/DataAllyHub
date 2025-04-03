using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace DataAllyEngine.AwsServices;

public class Sns
{
	public static async Task<string> PublishMessage(string topicArn, string message)
	{
		var snsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.USEast1);

		var publishRequest = new PublishRequest
		{
			TopicArn = topicArn,
			Message = message
		};

		var publishResponse = await snsClient.PublishAsync(publishRequest);
		return publishResponse.MessageId;
	}
}