using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Infrastructure.Caching.Base;
using StackExchange.Redis;

namespace RSPeer.Application.Features.Instance.Queries
{
	public class GetRunningClientsCountQueryHandler : IRequestHandler<GetRunningClientsCountQuery, long>
	{
		private readonly IRedisService _redis;

		public GetRunningClientsCountQueryHandler(IRedisService redis)
		{
			_redis = redis;
		}

		public async Task<long> Handle(GetRunningClientsCountQuery request, CancellationToken cancellationToken)
		{
			var tags = await _redis.GetSet($"{request.UserId}_running_client");
			if (tags.Count == 0)
			{
				return 0;
			}
			var keys = tags.Select(w => (RedisKey) $"{w}_client_details").ToArray();
			return await _redis.GetDatabase().KeyExistsAsync(keys);
		}
	}
}