using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
	public class GetUserByUsernameQuery : IRequest<User>
	{
		public string Username { get; set; }
		public bool IncludeGroups { get; set; }
	}
}