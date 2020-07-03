using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using MediatR;
using Microsoft.Extensions.Configuration;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.UserManagement.Users.Models;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Entities;
using RSPeer.Domain.Exceptions;
using RSPeer.Infrastructure.Cognito.Users.Commands;
using UserNotFoundException = Amazon.CognitoIdentityProvider.Model.UserNotFoundException;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class UserSignInCommandHandler : IRequestHandler<UserSignInCommand, UserSignInResult>
	{
		private readonly IMediator _mediator;
		private readonly IConfiguration _configuration;

		public UserSignInCommandHandler(IMediator mediator, IConfiguration configuration)
		{
			_mediator = mediator;
			_configuration = configuration;
		}

		public async Task<UserSignInResult> Handle(UserSignInCommand request, CancellationToken cancellationToken)
		{
			AuthenticationResultType result; 
			try
			{
				result = await _mediator.Send(new CognitoSignInCommand
				{
					Email = request.Email.ToLower().Trim(),
					Password = request.Password
				}, cancellationToken);
			}
			catch (AmazonCognitoIdentityProviderException e)
			{
				if (e is UserNotFoundException || e.Message.Contains("Incorrect username or password"))
				{
					throw new UserException("Incorrect username or password.");
				}

				throw;
			}

			if (result == null || string.IsNullOrEmpty(result.IdToken)) return null;

			var email = await _mediator.Send(new CognitoDecodeTokenCommand { Token = result.IdToken },
				cancellationToken);

			var user = await _mediator.Send(new GetUserByEmailQuery { Email = email, IncludeGroups = true },
				cancellationToken);

			if (user == null)
			{
				throw new NotFoundException("User", request.Email);
			}

			if (request.Type == LoginType.Forums)
			{
				var discourse = ValidateForumsSignature(request);
				discourse.User = user;
				var secret = _configuration.GetValue<string>("Forums:Token");
				var discourseSignInResult = new DiscourseSignInResult(discourse);
				DiscourseSignInResult.CalculateHash(discourseSignInResult, secret);
				return discourseSignInResult;
			}

			var payload = await _mediator.Send(new GetSignInPayloadQuery {User = user}, cancellationToken);
			return await _mediator.Send(new UserCreateSignInJwtCommand
			{
				Payload = payload
			}, cancellationToken);
		}

		private DiscourseSignInRequest ValidateForumsSignature(UserSignInCommand command)
		{
			var secret = _configuration.GetValue<string>("Forums:Token");
			var hash = HashingExtensions.SHA256(command.Sso, secret);

			if (hash != command.Sig)
			{
				throw new AuthorizationException("Invalid signature.");
			}
			
			var payload = Encoding.Default.GetString(Convert.FromBase64String(command.Sso));
			var qs = HttpUtility.ParseQueryString(payload);
			return new DiscourseSignInRequest
			{
				Nonce = qs["nonce"],
				Redirect = qs["return_sso_url"]
			};
		}
	}
}