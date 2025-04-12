using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using Amazon.S3;
using Amazon.S3.Transfer;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities.Encoders;

namespace DataAllyEngine.Common;

public static class ThumbnailTools
{
    private const int BIN_COUNT = 31;
    public const string DPA_SHA3_512_SIGNATURE = "8dcc5b03136153e14cf3fcfb2feed04e929d97e3dac829a589ce3273b717005b4fa281445e5f82f9ca305d7f4e703984c00b351f89b747a37ab8cda0d1075803";

    private static readonly HttpClient HttpClient = new HttpClient();

    public static Image FetchThumbnail(string url)
    {
        var response = HttpClient.GetAsync(url).Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to fetch thumbnail from {url}: status code {response.StatusCode}");
        }

        using var stream = response.Content.ReadAsStreamAsync().Result;
        return Image.FromStream(stream);
    }

    public static string GenerateGuid()
    {
        return Guid.NewGuid().ToString();
    }

    public static byte[] ConvertImageToBytes(Image image)
    {
        using var ms = new MemoryStream();
        var format = image.RawFormat ?? ImageFormat.Png;
        image.Save(ms, format);
        return ms.ToArray();
    }

    public static string GenerateSha3_512Signature(Image image)
    {
        var bytes = ConvertImageToBytes(image);
        var digest = new Sha3Digest(512);
        digest.BlockUpdate(bytes, 0, bytes.Length);
        var result = new byte[digest.GetDigestSize()];
        digest.DoFinal(result, 0);
        return Hex.ToHexString(result);
    }

    public static int HashToBinId(string guid)
    {
        using var md5 = MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(guid);
        var hash = md5.ComputeHash(bytes);
        var hashValue = BitConverter.ToUInt64(hash, 0);
        return (int)(hashValue % BIN_COUNT);
    }

    public static string ExtractFilenameFromUrl(string url)
    {
        return Path.GetFileName(new Uri(url).AbsolutePath);
    }

    public static string ExtractExtensionFromFilename(string filename)
    {
        return Path.GetExtension(filename).TrimStart('.').ToLowerInvariant();
    }

    public static string AssembleS3Key(string guid, string extension, int binId)
    {
        return $"{binId}/{guid}.{extension}";
    }

    public static void SaveThumbnail(byte[] image, string bucketArn, string s3Key)
    {
        using var stream = new MemoryStream(image);
        var bucketName = bucketArn.Contains(":")
            ? bucketArn[(bucketArn.LastIndexOf(":") + 1)..]
            : bucketArn;

        var s3Client = new AmazonS3Client();
        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            BucketName = bucketName,
            Key = s3Key
        };

        var fileTransferUtility = new TransferUtility(s3Client);
        fileTransferUtility.Upload(uploadRequest);
    }
}
