using MediatR;
using RSPeer.Application.Features.UserManagement.Users.Queries.Options;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
	public class GetUserByTokenQuery : IRequest<User>
	{
		public string Token { get; set; }
		public UserLookupOptions Options { get; set; }
	}
}