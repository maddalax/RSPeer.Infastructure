using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Infrastructure.Cognito.Users.Commands;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class ConvertCognitoJwtCommandHandler : IRequestHandler<ConvertCognitoJwtCommand, string>
	{
		private readonly IMediator _mediator;
		private readonly IRedisService _redis;
		private readonly ILogger<ConvertCognitoJwtCommandHandler> _logger;

		public ConvertCognitoJwtCommandHandler(IMediator mediator, IRedisService redis, ILogger<ConvertCognitoJwtCommandHandler> logger)
		{
			_mediator = mediator;
			_redis = redis;
			_logger = logger;
		}

		public async Task<string> Handle(ConvertCognitoJwtCommand request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrEmpty(request.CognitoJwt))
			{
				return null;
			}
			var key = $"jwt-mapping-{request.CognitoJwt}";
			// Fallback to support legacy aws cognito tokens.
			var userId = await _redis.Get<int?>(key);

			if (!userId.HasValue)
			{
				try
				{
					var email = await _mediator.Send(new CognitoDecodeTokenCommand { Token = request.CognitoJwt },
						cancellationToken);
					var user = await _mediator.Send(new GetUserByEmailQuery { Email = email }, cancellationToken);

					if (user == null)
					{
						return null;
					}

					userId = user.Id;
					await _redis.Set(key, user.Id, TimeSpan.FromHours(24));
					
				}
				catch (Exception e)
				{
					_logger.LogError(e, request.CognitoJwt);
					return null;
				}
			}
			
			var userById = await _mediator.Send(
				new GetUserByIdQuery { Id = userId.Value, IncludeGroups = true, AllowCached = request.AllowCachedUser },
				cancellationToken);
			var payload = await _mediator.Send(new GetSignInPayloadQuery { User = userById }, cancellationToken);
			var token = await _mediator.Send(new UserCreateSignInJwtCommand { Payload = payload }, cancellationToken);
			return token.Token;
		}
	}
}