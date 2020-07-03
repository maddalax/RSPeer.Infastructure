using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Application.Infrastructure.Caching.Commands;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class UserDisableCommandHandler : IRequestHandler<UserDisableCommand, Unit>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public UserDisableCommandHandler(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<Unit> Handle(UserDisableCommand request, CancellationToken cancellationToken)
		{
			var user = await _mediator.Send(new GetUserByIdQuery { Id = request.UserId, AllowCached = false }, cancellationToken);
			
			if (user == null)
			{
				return Unit.Value;
			}
			
			user.Disabled = request.Disabled;
			_db.Users.Update(user);
			await _db.SaveChangesAsync(cancellationToken);
			await _mediator.Send(new UpdateUserCacheCommand(user.Username), cancellationToken);
			return Unit.Value;
		}
	}
}