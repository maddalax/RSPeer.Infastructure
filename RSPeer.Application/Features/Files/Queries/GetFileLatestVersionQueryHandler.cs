using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Files.Queries
{
	public class GetFileLatestVersionQueryHandler : IRequestHandler<GetFileLatestVersionQuery, decimal>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;

		public GetFileLatestVersionQueryHandler(RsPeerContext db, IRedisService redis)
		{
			_db = db;
			_redis = redis;
		}

		public async Task<decimal> Handle(GetFileLatestVersionQuery request, CancellationToken cancellationToken)
		{
			var version = await _redis.GetDatabase().StringGetAsync($"file_{request.Name}_latest_version");

			if (version.HasValue && decimal.TryParse(version, out var parsed))
			{
				if (parsed > 0)
				{
					return parsed;
				}
			}
			
			var ver =  await _db.Files.Where(w => w.Name == request.Name)
				.OrderByDescending(w => w.Version)
				.Select(w => w.Version)
				.FirstOrDefaultAsync(cancellationToken);

			if (ver > 0)
			{
				await _redis.GetDatabase().StringSetAsync($"file_{request.Name}_latest_version", ver.ToString());
			}

			return ver;
		}
	}
}