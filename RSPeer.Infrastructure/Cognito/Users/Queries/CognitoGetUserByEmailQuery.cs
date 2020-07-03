using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Infrastructure.Cognito.Users.Queries
{
	public class CognitoGetUserByEmailQuery : IRequest<User>
	{
		public string Email { get; set; }
	}
}