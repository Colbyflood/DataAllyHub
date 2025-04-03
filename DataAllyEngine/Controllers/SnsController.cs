using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace DataAllyEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SnsController : ControllerBase
{
    // Subscribe the Endpoint to the SNS Topic:
    //
    // Go to the AWS SNS Console.
    // Select your topic.
    // Click on "Create subscription".
    // Set the protocol to HTTP or HTTPS.
    // Enter the URL of your .NET application's endpoint.
    // Click "Create subscription".
    
    // Step 2: Configure the Application
    //
    // Run the Application: Start your ASP.NET Core application. Ensure it's accessible from the internet
    // if you're using HTTP (consider using a tool like ngrok for local development).
    //
    // Confirm the Subscription: When SNS sends a subscription confirmation message, handle it by
    // extracting the SubscribeURL from the message and sending an HTTP GET request to confirm the subscription.
    //
    // Considerations
    //
    // Security: Ensure your endpoint is secured. Consider using HTTPS and verifying SNS
    // signatures to ensure messages are genuinely from AWS SNS.
    // Scalability: If you expect a high volume of messages, ensure your application is
    // designed to handle them efficiently.
    //
    // Error Handling: Implement robust error handling and logging to manage any issues
    // with message processing.
    //
    // By following these steps, you can set up a .NET application to receive alerts when an
    // SNS message is sent. Adjust your application logic to process the messages according to your specific requirements.

    [HttpPost]
    public async Task<IActionResult> ReceiveSnsMessage()
    {
        using (var reader = new StreamReader(Request.Body))
        {
            var message = await reader.ReadToEndAsync();
            // Process the message
            Console.WriteLine("Received SNS message: " + message);

            // Handle the subscription confirmation
            if (Request.Headers.ContainsKey("x-amz-sns-message-type") &&
                Request.Headers["x-amz-sns-message-type"] == "SubscriptionConfirmation")
            {
                // Extract the SubscribeURL from the message and confirm the subscription
                // This step requires additional parsing to extract and confirm the URL
            }
        }

        return Ok();
    }
}
