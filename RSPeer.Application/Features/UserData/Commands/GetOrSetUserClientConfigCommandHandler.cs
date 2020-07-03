using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.UserData.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserData.Commands
{
    public class GetOrSetUserClientConfigCommandHandler : IRequestHandler<GetOrSetUserClientConfigCommand, UserClientConfig>
    {
        private const string USER_DATA_KEY = "user_client_config";
        
        private readonly IMediator _mediator;
        private readonly IRedisService _redis;

        public GetOrSetUserClientConfigCommandHandler(IMediator mediator, IRedisService redis)
        {
            _mediator = mediator;
            _redis = redis;
        }

        public async Task<UserClientConfig> Handle(GetOrSetUserClientConfigCommand request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;
            var redisKey = $"{USER_DATA_KEY}_{userId}";

            var data = await _redis.GetOrDefault(redisKey,
                async () =>
                {
                    var json = await _mediator.Send(new GetUserJsonDataQuery {Key = USER_DATA_KEY, UserId = userId},
                        cancellationToken);
                    return json == null ? null : JsonSerializer.Deserialize<UserClientConfig>(json);
                }, TimeSpan.FromHours(24));
            
            
            if (data != null)
            {
                return data;
            }

            var channel = $"{StringExtensions.GetUniqueKey(60)}_{userId}";
            channel = channel.Length > 64 ? channel.Substring(channel.Length - 64) : channel;
            
            var defaultConfig = new UserClientConfig
            {
                Channel = channel
            };

            await _mediator.Send(new SaveUserJsonDataCommand
            {
                Key = USER_DATA_KEY,
                UserId = userId,
                Value = defaultConfig
            }, cancellationToken);

            await _redis.SetJson(redisKey, defaultConfig, TimeSpan.FromHours(24));

            return defaultConfig;
        }
    }
}