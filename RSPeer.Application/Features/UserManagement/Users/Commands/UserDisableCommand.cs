using MediatR;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class UserDisableCommand : IRequest<Unit>
	{
		public int UserId { get; set; }
		public bool Disabled { get; set; }
	}
}