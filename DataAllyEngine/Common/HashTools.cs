using Org.BouncyCastle.Crypto.Digests;
using System.Text;

namespace DataAllyEngine.Common;

public static class HashTools
{
	public static string HashTitleAndBody(string title, string body)
	{
		var digest = new Sha3Digest(256);

		var titleBytes = Encoding.UTF8.GetBytes(title);
		digest.BlockUpdate(titleBytes, 0, titleBytes.Length);

		var bodyBytes = Encoding.UTF8.GetBytes(body);
		digest.BlockUpdate(bodyBytes, 0, bodyBytes.Length);

		var result = new byte[digest.GetDigestSize()];
		digest.DoFinal(result, 0);

		return BitConverter.ToString(result).Replace("-", "").ToLowerInvariant();
	}
}