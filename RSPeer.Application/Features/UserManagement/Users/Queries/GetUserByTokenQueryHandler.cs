using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.UserManagement.Users.Commands;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
	public class GetUserByTokenQueryHandler : IRequestHandler<GetUserByTokenQuery, User>
	{
		private readonly IMediator _mediator;

		public GetUserByTokenQueryHandler(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<User> Handle(GetUserByTokenQuery request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrEmpty(request.Token))
			{
				return null;
			}

			var user = await _mediator.Send(new UserDecodeSignInJwtCommand { Token = request.Token }, cancellationToken);
			
			if (user == null)
			{
				return null;
			}

			if (request.Options != null && request.Options.FullUser)
			{
				return await _mediator.Send(new GetUserByIdQuery { Id = user.Id, AllowCached = false }, cancellationToken);
			}
			
			if (request.Options != null && request.Options.IncludeBalance)
			{
				return await _mediator.Send(new GetUserByIdQuery { Id = user.Id, AllowCached = false }, cancellationToken);
			}

			return user;
		}
	}
}