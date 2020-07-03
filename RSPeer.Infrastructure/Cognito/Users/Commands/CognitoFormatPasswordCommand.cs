using MediatR;

namespace RSPeer.Infrastructure.Cognito.Users.Commands
{
	public class CognitoFormatPasswordCommand : IRequest<Unit>
	{
		public string Email { get; set; }
	}
}