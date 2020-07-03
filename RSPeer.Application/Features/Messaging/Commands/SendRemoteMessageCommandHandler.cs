using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Entities;
using StackExchange.Redis;

namespace RSPeer.Application.Features.Messaging.Commands
{
    public class SendRemoteMessageCommandHandler : IRequestHandler<SendRemoteMessageCommand, Unit>
    {
        private readonly IRedisService _redis;

        public SendRemoteMessageCommandHandler(IRedisService redis)
        {
            _redis = redis;
        }

        public async Task<Unit> Handle(SendRemoteMessageCommand request, CancellationToken cancellationToken)
        {
            var message = new RemoteMessage
            {
                UserId = request.UserId,
                Consumer = request.Consumer,
                Timestamp = DateTimeOffset.Now,
                Message = request.Message,
                Source = request.Source,
                Id = RandomExtensions.RandomNumber()
            };
            
            var transaction = _redis.GetDatabase().CreateTransaction();
            var key = $"messages_{request.UserId}";
            
            transaction.SetAddAsync(key, message.Id);
            transaction.StringSetAsync($"messages_{request.UserId}_{message.Id}",
                JsonSerializer.Serialize(message), TimeSpan.FromMinutes(10));

            await transaction.ExecuteAsync(CommandFlags.FireAndForget);
            return Unit.Value;
        }
    }
}