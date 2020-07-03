using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.UserManagement.Users.Models;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
	public class GetSignInPayloadQueryHandler : IRequestHandler<GetSignInPayloadQuery, UserSignInResultPayload>
	{
		public Task<UserSignInResultPayload> Handle(GetSignInPayloadQuery request, CancellationToken cancellationToken)
		{
			return Task.FromResult(new UserSignInResultPayload
			{
				SignInDate = DateTimeOffset.UtcNow,
				User = request.User,
				Nonce = Guid.NewGuid()
			});
		}
	}
}