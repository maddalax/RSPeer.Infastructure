using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace RSPeer.Infrastructure.Cognito.Users.Models
{
	public class JwtDecryptionKeys
	{
		[JsonPropertyName("keys")] public List<JwtDecryptionKey> Keys { get; set; }
	}

	public class JwtDecryptionKey
	{
		[JsonPropertyName("alg")] public string Alg { get; set; }

		[JsonPropertyName("e")] public string E { get; set; }

		[JsonPropertyName("kid")] public string Kid { get; set; }

		[JsonPropertyName("kty")] public string Kty { get; set; }

		[JsonPropertyName("n")] public string N { get; set; }

		[JsonPropertyName("use")] public string Use { get; set; }
	}
}