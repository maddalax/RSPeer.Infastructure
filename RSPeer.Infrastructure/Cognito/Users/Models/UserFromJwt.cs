using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace RSPeer.Infrastructure.Cognito.Users.Models
{
	public class UserFromJwt
	{
		[JsonPropertyName("sub")] public string UserId { get; set; }

		[JsonPropertyName("aud")] public string Aud { get; set; }

		[JsonPropertyName("cognito:groups")] public List<string> Groups { get; set; }

		[JsonPropertyName("email_verified")] public bool IsEmailVerified { get; set; }

		[JsonPropertyName("event_id")] public string EventId { get; set; }

		[JsonPropertyName("token_use")] public string TokenUse { get; set; }

		[JsonPropertyName("auth_time")] public long AuthTime { get; set; }

		[JsonPropertyName("iss")] public string Iss { get; set; }

		[JsonPropertyName("preferred_username")] public string Username { get; set; }

		[JsonPropertyName("exp")] public long Exp { get; set; }

		[JsonPropertyName("iat")] public long Iat { get; set; }

		[JsonPropertyName("email")] public string Email { get; set; }
	}
}