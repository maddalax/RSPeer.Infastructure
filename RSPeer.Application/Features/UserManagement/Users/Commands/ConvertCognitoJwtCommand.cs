using MediatR;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class ConvertCognitoJwtCommand : IRequest<string>
	{
		public string CognitoJwt { get; set; }
		public bool AllowCachedUser { get; set; }
	}
}