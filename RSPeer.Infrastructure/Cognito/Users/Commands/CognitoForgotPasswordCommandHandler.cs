using System;
using System.Collections.Generic;
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
	public class CognitoForgotPasswordCommandHandler : BaseCognitoHandler<CognitoFormatPasswordCommand, Unit>
	{
		private readonly IMediator _mediator;

		public CognitoForgotPasswordCommandHandler(IConfiguration configuration, IMediator mediator) : base(configuration)
		{
			_mediator = mediator;
		}

		public override async Task<Unit> Handle(CognitoFormatPasswordCommand request, CancellationToken cancellationToken)
		{
			var user = await _mediator.Send(new CognitoGetUserByEmailQuery { Email = request.Email }, cancellationToken);

			if (user == null)
			{
				throw new Exception("User not found by email: " + request.Email + ". Unable to reset password.");
			}
			
			using (var provider = new AmazonCognitoIdentityProviderClient(AwsId, AwsKey, RegionEndpoint.USEast1))
			{
				if (!user.IsEmailVerified)
				{
					await provider.AdminUpdateUserAttributesAsync(new AdminUpdateUserAttributesRequest
					{
						UserAttributes = new List<AttributeType>
						{
							new AttributeType { Name = "email_verified", Value = "true" }
						},
						Username = user.LegacyId.ToString(),
						UserPoolId = UserPoolId
					}, cancellationToken);			
				}
				await provider.ForgotPasswordAsync(new ForgotPasswordRequest
				{
					ClientId = UserGroupClientId,
					Username = user.LegacyId.ToString()
				}, cancellationToken);
			}
			
			return Unit.Value;
		}
	}
}