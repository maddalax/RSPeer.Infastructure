using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RSPeer.Common.Extensions
{
	public static class StreamExtensions
	{
		public static byte[] ToByteArray(this Stream stream)
		{
			using (var ms = new MemoryStream())
			{
				stream.Position = 0;
				stream.CopyTo(ms);
				stream.Close();
				return ms.ToArray();
			}
		}

		public static async Task<string> ToStringAsync(this Stream stream)
		{
			using (var reader = new StreamReader(stream))
			{
				return await reader.ReadToEndAsync();
			}
		}
		
		public static string CalculateHash(this Stream stream)
		{
			var provider = new SHA512CryptoServiceProvider();
			var hashedBytes = provider.ComputeHash(stream);
			return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
		}
	}
}