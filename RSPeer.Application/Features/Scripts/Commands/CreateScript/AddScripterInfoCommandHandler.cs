using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using System.Text.Json;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Domain.Entities;
using RSPeer.Infrastructure.Gitlab.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripts.Commands.CreateScript
{
	public class AddScripterInfoCommandHandler : IRequestHandler<AddScripterInfoCommand, Unit>
	{
		private readonly IGitlabService _gitlab;
		private readonly IMediator _mediator;
		private readonly RsPeerContext _db;

		public AddScripterInfoCommandHandler(IGitlabService gitlab, IMediator mediator, RsPeerContext db)
		{
			_gitlab = gitlab;
			_mediator = mediator;
			_db = db;
		}

		public async Task<Unit> Handle(AddScripterInfoCommand request, CancellationToken cancellationToken)
		{
			var gitlabUser = await _gitlab.GetUserById(request.GitlabId);
			var user = await _mediator.Send(new GetUserByIdQuery { Id = request.UserId }, 
				cancellationToken);

			var group = await _gitlab.CreateScripterGroup(user, gitlabUser);
			
			var info = new ScripterInfo
			{
				DateAdded = DateTime.UtcNow,
				GitlabGroupPath = $"https://gitlab.com/rspeer-public-sdn/{user.Username}/",
				GitlabUserId = gitlabUser.Id,
				GitlabUsername = gitlabUser.Username,
				UserId = user.Id,
				GitlabGroupId = group.Id
			};

			await _db.ScripterInfo.AddAsync(info, cancellationToken);
			await _db.SaveChangesAsync(cancellationToken);
			
			return Unit.Value;
		}
	}
}