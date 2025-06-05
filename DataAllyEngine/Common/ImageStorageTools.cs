using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using Amazon.S3;
using Amazon.S3.Transfer;
// using Org.BouncyCastle.Crypto.Digests;
// using Org.BouncyCastle.Utilities.Encoders;

namespace DataAllyEngine.Common;

public static class ImageStorageTools
{
    // ReSharper disable InconsistentNaming
    private const int THUMBNAIL_BIN_COUNT = 31;
    private const int CREATIVE_BIN_COUNT = 10007;

    private static readonly HttpClient HttpClient = new HttpClient();

    public static MemoryStream FetchFileToMemory(string url, string extension = "png")
    {
        var response = HttpClient.GetAsync(url).Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to fetch image/video file from {url}: status code {response.StatusCode}");
        }

        using (var imageStream = response.Content.ReadAsStreamAsync().Result)
        {
            var memoryStream = new MemoryStream();
            if (IsImage(extension))
            {
                using (var bitmap = new Bitmap(imageStream))
                {
                    bitmap.Save(memoryStream, GetImageFormat(extension));
                }
            }
            else
            {
                imageStream.CopyTo(memoryStream);
            }
            memoryStream.Position = 0;
            return memoryStream; 
        }
    }

    public static string GenerateGuid()
    {
        return Guid.NewGuid().ToString();
    }
    //
    // public static byte[] ConvertImageToBytes(Image image)
    // {
    //     using var ms = new MemoryStream();
    //     var format = image.RawFormat ?? ImageFormat.Png;
    //     image.Save(ms, format);
    //     return ms.ToArray();
    // }

    // public static string GenerateSha3_512Signature(MemoryStream imageStream)
    // {
    //     imageStream.Position = 0;
    //     var bytes = imageStream.ToArray();
    //     var digest = new Sha3Digest(512);
    //     digest.BlockUpdate(bytes, 0, bytes.Length);
    //     var result = new byte[digest.GetDigestSize()];
    //     digest.DoFinal(result, 0);
    //     return Hex.ToHexString(result);
    // }

    public static int HashThumbnailToBinId(string guid)
    {
        using var md5 = MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(guid);
        var hash = md5.ComputeHash(bytes);
        var hashValue = BitConverter.ToUInt64(hash, 0);
        return (int)(hashValue % THUMBNAIL_BIN_COUNT);
    }
    
    public static int HashCreativeToBinId(string guid)
    {
        using var md5 = MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(guid);
        var hash = md5.ComputeHash(bytes);
        var hashValue = BitConverter.ToUInt64(hash, 0);
        return (int)(hashValue % CREATIVE_BIN_COUNT);
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

    public static void SaveImageToS3(IAmazonS3 s3Client, MemoryStream imageStream, string bucketArn, string s3Key)
    {
        imageStream.Position = 0;

        var bucketName = bucketArn.Contains(":")
            ? bucketArn[(bucketArn.LastIndexOf(":") + 1)..]
            : bucketArn;

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = imageStream,
            BucketName = bucketName,
            Key = s3Key
        };

        var fileTransferUtility = new TransferUtility(s3Client);
        fileTransferUtility.Upload(uploadRequest);
    }
    
    public static string DeriveExtensionFromFilename(string? filename)
    {
        if (!string.IsNullOrWhiteSpace(filename))
        {
            var ext = ImageStorageTools.ExtractExtensionFromFilename(filename).ToUpper();
            if (ext.StartsWith("PNG")) return "png";
            if (ext.StartsWith("JPEG") || ext.StartsWith("JPG")) return "jpg";
            if (ext.StartsWith("GIF")) return "gif";
            if (ext.StartsWith("BMP")) return "bmp";
            if (ext.StartsWith("MPEG") || ext.StartsWith("MPG")) return "mpeg";
            if (ext.StartsWith("MP4")) return "mp4";
            if (ext.StartsWith("MOV")) return "mov";
            if (ext.StartsWith("WEBM")) return "webm";
            if (ext.StartsWith("AVI")) return "avi";
        }
        return "png";
    }
    
    public static bool IsImage(string extension)
    {
        if (!string.IsNullOrWhiteSpace(extension))
        {
            var ext = extension.ToUpper();
            if (ext.StartsWith("PNG")) return true;
            if (ext.StartsWith("JPEG") || ext.StartsWith("JPG")) return true;
            if (ext.StartsWith("GIF")) return true;
            if (ext.StartsWith("BMP")) return true;
        }
        return false;
    }

    private static ImageFormat GetImageFormat(string extension)
    {
        if (!string.IsNullOrWhiteSpace(extension))
        {
            var ext = extension.ToUpper();
            if (ext.StartsWith("PNG")) return ImageFormat.Png;
            if (ext.StartsWith("JPEG") || ext.StartsWith("JPG")) return ImageFormat.Jpeg;
            if (ext.StartsWith("GIF")) return ImageFormat.Gif;
            if (ext.StartsWith("BMP")) return ImageFormat.Bmp;
        }
        return ImageFormat.Png;
    }
}
