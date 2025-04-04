using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace DataAllyEngine.AwsServices;

public class S3
{
    public static async Task<string> ReadTextFile(string bucketName, string fileName)
    {
        var s3Client = new AmazonS3Client(RegionEndpoint.USEast1);

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            using var response = await s3Client.GetObjectAsync(request);
            using var reader = new StreamReader(response.ResponseStream);
            return await reader.ReadToEndAsync();
        }
        catch (AmazonS3Exception e)
        {
            throw new Exception($"Amazon S3 error on server. Message:'{e.Message}' when writing an object");
        }
        catch (Exception e)
        {
            throw new Exception($"Unknown error on server. Message:'{e.Message}' when writing an object");
        }
    }
}
