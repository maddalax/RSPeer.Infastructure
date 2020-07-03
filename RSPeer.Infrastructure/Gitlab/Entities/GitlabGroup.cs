using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace RSPeer.Infrastructure.Gitlab.Entities
{
	public class GitlabGroup
	{
		[JsonPropertyName("id")] public long Id { get; set; }

		[JsonPropertyName("web_url")] public string WebUrl { get; set; }

		[JsonPropertyName("name")] public string Name { get; set; }

		[JsonPropertyName("path")] public string Path { get; set; }

		[JsonPropertyName("description")] public string Description { get; set; }

		[JsonPropertyName("visibility")] public string Visibility { get; set; }

		[JsonPropertyName("lfs_enabled")] public bool LfsEnabled { get; set; }

		[JsonPropertyName("avatar_url")] public object AvatarUrl { get; set; }

		[JsonPropertyName("request_access_enabled")]
		public bool RequestAccessEnabled { get; set; }

		[JsonPropertyName("full_name")] public string FullName { get; set; }

		[JsonPropertyName("full_path")] public string FullPath { get; set; }

		[JsonPropertyName("parent_id")] public long ParentId { get; set; }

		[JsonPropertyName("projects")] public List<object> Projects { get; set; }

		[JsonPropertyName("shared_projects")] public List<object> SharedProjects { get; set; }

		[JsonPropertyName("ldap_cn")] public object LdapCn { get; set; }

		[JsonPropertyName("ldap_access")] public object LdapAccess { get; set; }

		[JsonPropertyName("shared_runners_minutes_limit")]
		public object SharedRunnersMinutesLimit { get; set; }
	}
}