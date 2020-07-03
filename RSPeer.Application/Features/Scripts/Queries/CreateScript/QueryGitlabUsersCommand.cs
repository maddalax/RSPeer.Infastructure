using System.Collections.Generic;
using MediatR;
using RSPeer.Infrastructure.Gitlab.Entities;

namespace RSPeer.Application.Features.Scripts.Queries.CreateScript
{
	public class QueryGitlabUsersCommand : IRequest<IEnumerable<GitlabUser>>
	{
		public string GitlabUsername { get; set; }
	}
}