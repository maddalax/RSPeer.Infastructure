using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Scripts.Commands.CreateScript;
using RSPeer.Infrastructure.Gitlab.Base;
using RSPeer.Infrastructure.Gitlab.Entities;

namespace RSPeer.Application.Features.Scripts.Queries.CreateScript
{
	public class QueryGitlabUsersCommandHandler : IRequestHandler<QueryGitlabUsersCommand, IEnumerable<GitlabUser>>
	{
		private readonly IGitlabService _service;

		public QueryGitlabUsersCommandHandler(IGitlabService service)
		{
			_service = service;
		}

		public async Task<IEnumerable<GitlabUser>> Handle(QueryGitlabUsersCommand request, CancellationToken cancellationToken)
		{
			return await _service.QueryUsers(request.GitlabUsername);
		}
	}
}