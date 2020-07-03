using System.ComponentModel.DataAnnotations;
using MediatR;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class UserResetPasswordConfirmCommand : IRequest<Unit>
	{
		[Required] public string Code { get; set; }

		[Required] public string Email { get; set; }

		[Required] public string Password { get; set; }
	}
}