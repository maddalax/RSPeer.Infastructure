using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.ScriptAccesses.Commands
{
	public class ClearScriptAccessCacheCommandHandler : IRequestHandler<ClearScriptAccessCacheCommand, Unit>
	{
		private readonly IRedisService _redis;

		public ClearScriptAccessCacheCommandHandler(IRedisService redis)
		{
			_redis = redis;
		}

		public async Task<Unit> Handle(ClearScriptAccessCacheCommand request, CancellationToken cancellationToken)
		{
			var featured = Enum.GetName(typeof(ScriptOrderBy), ScriptOrderBy.Featured);
			var allTime = Enum.GetName(typeof(ScriptOrderBy), ScriptOrderBy.FeaturedAllTime);
			var type = Enum.GetName(typeof(ScriptType), ScriptType.Mine);
			var key = $"script:sdn:mostPopular:{featured}:{type}::{request.UserId}";
			await _redis.Remove(
				key);
			await _redis.Remove(
				$"script:sdn:mostPopular:{allTime}:{type}::{request.UserId}");
			foreach (var name in Enum.GetNames(typeof(ScriptCategory)))
			{
				await _redis.Remove(
					$"script:sdn:mostPopular:{allTime}:{type}:{name}:{request.UserId}");
				await _redis.Remove(
					$"script:sdn:mostPopular:{featured}:{type}:{name}:{request.UserId}");
			}
			return Unit.Value;
		}
	}
}