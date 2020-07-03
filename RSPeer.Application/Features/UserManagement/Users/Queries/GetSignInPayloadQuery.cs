using MediatR;
using RSPeer.Application.Features.UserManagement.Users.Models;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
	public class GetSignInPayloadQuery : IRequest<UserSignInResultPayload>
	{
		public User User { get; set; }
	}
}