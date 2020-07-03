using System.Text.Json.Serialization;

namespace RSPeer.Infrastructure.Gitlab.Entities
{
	public class GitlabUser
	{
		[JsonPropertyName("id")] public int Id { get; set; }

		[JsonPropertyName("name")] public string Name { get; set; }

		[JsonPropertyName("username")] public string Username { get; set; }

		[JsonPropertyName("web_url")] public string Url { get; set; }
	}
}