using MediatR;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class UserResetPasswordCommand : IRequest<Unit>
	{
		public string Email { get; set; }
	}
}