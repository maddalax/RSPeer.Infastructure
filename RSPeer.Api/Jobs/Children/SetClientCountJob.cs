using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Instance.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Jobs.Children
{
	public class SetClientCountJob
	{
		private readonly IRedisService _redis;
		private readonly IMediator _mediator;

		public SetClientCountJob(IRedisService redis, IMediator mediator)
		{
			_redis = redis;
			_mediator = mediator;
		}

		public async Task Execute()
		{
			var osrs = await _redis.GetSet($"users_running_clients_{Game.Osrs.ToString()}");
			var rs3 = await _redis.GetSet($"users_running_clients_{Game.Rs3.ToString()}");
			var userIds = osrs.Concat(rs3);
			long total = 0;
			foreach (var userId in userIds)
			{
				total += await _mediator.Send(new GetRunningClientsCountQuery {UserId = int.Parse(userId)});
			}
			await _redis.Set("connected_client_count", Convert.ToInt64(total * 1.45));
		}

	}
}