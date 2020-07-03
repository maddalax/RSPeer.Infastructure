using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jose;
using MediatR;
using Microsoft.Extensions.Configuration;
using RSPeer.Application.Features.UserManagement.Users.Models;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class UserDecodeSignInJwtCommandHandler : IRequestHandler<UserDecodeSignInJwtCommand, User>
	{
		private readonly byte[] _secret;
		private readonly IMediator _mediator;

		public UserDecodeSignInJwtCommandHandler(IConfiguration configuration, IMediator mediator)
		{
			_secret = Encoding.UTF8.GetBytes(configuration.GetValue<string>("Jwt:Secret"));
			_mediator = mediator;
		}

		public async Task<User> Handle(UserDecodeSignInJwtCommand request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrEmpty(request.Token))
			{
				return null;
			}
			try
			{
				return Decode(request.Token).User;
			}
			catch (Exception)
			{
				var token = await _mediator.Send(new ConvertCognitoJwtCommand { CognitoJwt = request.Token }, cancellationToken);
				if (token != null)
				{
					return Decode(token).User;
				}
			}

			return null;
		}

		private UserSignInResultPayload Decode(string token)
		{
			return string.IsNullOrEmpty(token) 
				? null 
				: JWT.Decode<UserSignInResultPayload>(token, _secret, JwsAlgorithm.HS512);
		}
	}
}