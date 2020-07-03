using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Infrastructure.Caching.Base;
using StackExchange.Redis;

namespace RSPeer.Application.Features.Instance.Commands
{
	public class SaveClientInfoCommandHandler : IRequestHandler<SaveClientInfoBulkCommand, Unit>
	{
		private readonly IRedisService _redis;

		public SaveClientInfoCommandHandler(IRedisService redis)
		{
			_redis = redis;
		}

		public async Task<Unit> Handle(SaveClientInfoBulkCommand request, CancellationToken cancellationToken)
		{
			var transaction = _redis.GetDatabase().CreateTransaction();
			foreach (var client in request.Clients)
			{
				transaction.SetAddAsync($"users_running_clients_{client.Game.ToString()}", client.UserId);
				transaction.SetAddAsync($"{client.UserId}_running_client", client.Tag.ToString());
				transaction.StringSetAsync($"{client.Tag}_client_details",
					JsonSerializer.Serialize(client), TimeSpan.FromMinutes(5));
			}

			await transaction.ExecuteAsync(CommandFlags.FireAndForget);
			return Unit.Value;
		}
	}
}