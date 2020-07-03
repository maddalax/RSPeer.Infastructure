using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RSPeer.Common.Extensions
{
	public static class StringExtensions
	{
		public static T To<T>(this string value)
		{
			return value == null ? default : JsonSerializer.Deserialize<T>(value);
		}

		public static byte[] Xor(byte[] data, string key) {
			byte[] result = new byte[data.Length];
			byte[] keyByte = Encoding.UTF8.GetBytes(key);
			for (int x = 0, y = 0; x < data.Length; x++, y++) {
				if (y == keyByte.Length) {
					y = 0;
				}
				result[x] = (byte) (data[x] ^ keyByte[y]);
			}
			return result;
		}
		
		
		public static byte[] Xor(string payload, string key)
		{
			var data = Encoding.UTF8.GetBytes(payload);
			byte[] result = new byte[data.Length];
			byte[] keyByte = Encoding.UTF8.GetBytes(key);
			for (int x = 0, y = 0; x < data.Length; x++, y++) {
				if (y == keyByte.Length) {
					y = 0;
				}
				result[x] = (byte) (data[x] ^ keyByte[y]);
			}
			return result;
		}

		public static string Sha256Hash(this string content)
		{
			using (var sha256Hash = SHA256.Create())  
			{  
				var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(content));  
				var builder = new StringBuilder();  
				foreach (var t in bytes)
				{
					builder.Append(t.ToString("x2"));
				}  
				return builder.ToString();  
			}  
		}
		
		public static string GetUniqueKey(int size)
		{
			var chars =
				"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
			var data = new byte[size];
			using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
			{
				crypto.GetBytes(data);
			}
			var result = new StringBuilder(size);
			foreach (byte b in data)
			{
				result.Append(chars[b % (chars.Length)]);
			}
			return result.ToString();
		}
	}
}