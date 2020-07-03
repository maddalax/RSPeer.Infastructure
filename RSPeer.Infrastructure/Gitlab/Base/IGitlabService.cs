using System.Collections.Generic;
using System.Threading.Tasks;
using RSPeer.Domain.Entities;
using RSPeer.Infrastructure.Gitlab.Entities;

namespace RSPeer.Infrastructure.Gitlab.Base
{
	public interface IGitlabService
	{
		Task<GitlabGroup> CreateScripterGroup(User user, GitlabUser gitlabUser);
		Task<List<GitlabUser>> QueryUsers(string username);
		Task<GitlabUser> GetUserById(int userId);
		Task AddUserToGroup(long groupId, long userId);
	}
}