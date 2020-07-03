using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RSPeer.Domain.Entities;
using RSPeer.Infrastructure.Gitlab.Base;
using RSPeer.Infrastructure.Gitlab.Entities;

namespace RSPeer.Infrastructure.Gitlab
{
	public class GitLabService : IGitlabService
	{
		private readonly HttpClient _client;
		private readonly string _repoGroupId;

		public GitLabService(IConfiguration configuration, IHttpClientFactory factory)
		{
			_client = factory.CreateClient("Gitlab");
			var token = configuration.GetValue<string>("GitLab:PrivateToken");
			_repoGroupId = configuration.GetValue<string>("GitLab:RepoGroupId");
			var baseUrl = configuration.GetValue<string>("Gitlab:BaseUrl");
			_client.BaseAddress = new Uri(baseUrl);
			_client.DefaultRequestHeaders.Add("Private-Token", token);
		}

		public async Task<GitlabGroup> CreateScripterGroup(User user, GitlabUser gitlabUser)
		{
			var username = user.Username;

			if (gitlabUser == null)
			{
				throw new Exception("Unable to find GitLab user, invalid user id.");
			}

			var description = $"{username}'s Scripts";

			var route =
				$"groups?name={username}&path={username.Replace(" ", "")}&description={description}&visibility=private&parent_id={_repoGroupId}";

			var res = await _client.PostAsync(route, null);

			var content = await res.Content.ReadAsStringAsync();

			if (!res.IsSuccessStatusCode)
			{
				var parsed = JsonSerializer.Deserialize<JObject>(content);
				throw new Exception(parsed["message"].Value<string>());
			}

			var createdGroup = JsonSerializer.Deserialize<GitlabGroup>(content);

			await AddUserToGroup(createdGroup.Id, gitlabUser.Id);

			return createdGroup;
		}

		public async Task<List<GitlabUser>> QueryUsers(string username)
		{
			var res = await _client.GetAsync($"users?search={username}");
			var content = await res.Content.ReadAsStringAsync();
			if (!res.IsSuccessStatusCode)
			{
				var parsed = JsonSerializer.Deserialize<JObject>(content);
				throw new Exception(parsed["message"].Value<string>());
			}

			var users = JsonSerializer.Deserialize<List<GitlabUser>>(content);
			return users.Count == 0 ? new List<GitlabUser>() : users;
		}

		public async Task<GitlabUser> GetUserById(int userId)
		{
			var res = await _client.GetAsync($"users/{userId}");
			var content = await res.Content.ReadAsStringAsync();
			if (!res.IsSuccessStatusCode)
			{
				var parsed = JsonSerializer.Deserialize<JObject>(content);
				throw new Exception(parsed["message"].Value<string>());
			}

			return JsonSerializer.Deserialize<GitlabUser>(content);
		}

		public async Task AddUserToGroup(long groupId, long userId)
		{
			var res = await _client.PostAsync($"groups/{groupId}/members?user_id={userId}&access_level=40", null);
			if (!res.IsSuccessStatusCode)
			{
				var content = await res.Content.ReadAsStringAsync();
				throw new Exception(content);
			}
		}
	}
}