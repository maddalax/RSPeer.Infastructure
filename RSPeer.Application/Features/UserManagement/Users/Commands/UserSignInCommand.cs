using MediatR;
using RSPeer.Application.Features.UserManagement.Users.Models;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class UserSignInCommand : IRequest<UserSignInResult>
	{
		public string Email { get; set; }
		public string Password { get; set; }

		public LoginType Type { get; set; } = LoginType.Unknown;
		
		public string Sso { get; set; }
		
		public string Sig { get; set; }
	}
}