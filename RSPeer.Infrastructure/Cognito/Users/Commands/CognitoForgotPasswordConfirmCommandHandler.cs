using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using MediatR;
using Microsoft.Extensions.Configuration;
using RSPeer.Infrastructure.Cognito.Users.Base;
using RSPeer.Infrastructure.Cognito.Users.Queries;

namespace RSPeer.Infrastructure.Cognito.Users.Commands
{
	public class CognitoForgotPasswordConfirmCommandHandler : BaseCognitoHandler<CognitoForgotPasswordConfirmCommand, Unit>
	{
		private readonly IMediator _mediator;

		public CognitoForgotPasswordConfirmCommandHandler(IConfiguration configuration, IMediator mediator) : base(
			configuration)
		{
			_mediator = mediator;
		}

		public override async Task<Unit> Handle(CognitoForgotPasswordConfirmCommand request,
			CancellationToken cancellationToken)
		{
			var user = await _mediator.Send(new CognitoGetUserByEmailQuery { Email = request.Email },
				cancellationToken);

			if (user == null)
			{
				throw new Exception("User not found by email: " + request.Email +
				                    ". Unable to confirm reset password.");
			}

			using (var provider = new AmazonCognitoIdentityProviderClient(AwsId, AwsKey, RegionEndpoint.USEast1))
			{
				var reset = await provider.ConfirmForgotPasswordAsync(new ConfirmForgotPasswordRequest
				{
					ClientId = UserGroupClientId,
					Password = request.Password,
					ConfirmationCode = request.Code,
					Username = user.LegacyId.ToString()
				}, cancellationToken);

				if (reset.HttpStatusCode != HttpStatusCode.OK)
				{
					throw new Exception("Failed to reset password. Status Code: " + reset.HttpStatusCode);
				}

				return Unit.Value;
			}
		}
	}
}