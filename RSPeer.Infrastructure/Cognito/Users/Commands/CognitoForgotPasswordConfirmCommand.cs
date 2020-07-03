using System.ComponentModel.DataAnnotations;
using MediatR;

namespace RSPeer.Infrastructure.Cognito.Users.Commands
{
	public class CognitoForgotPasswordConfirmCommand : IRequest<Unit>
	{
		[Required] public string Code { get; set; }

		[Required] public string Email { get; set; }

		[Required] public string Password { get; set; }
	}
}