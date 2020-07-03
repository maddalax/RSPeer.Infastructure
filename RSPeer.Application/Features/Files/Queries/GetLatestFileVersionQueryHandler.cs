using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Files.Queries
{
	public class GetLatestFileVersionQueryHandler : IRequestHandler<GetLatestFileVersionQuery, decimal>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;

		public GetLatestFileVersionQueryHandler(RsPeerContext db, IRedisService redis)
		{
			_db = db;
			_redis = redis;
		}

		public async Task<decimal> Handle(GetLatestFileVersionQuery request, CancellationToken cancellationToken)
		{
			return await _redis.GetOrDefault($"file_{request.Name}_latest_version", async () =>
			{
				return await _db.Files.Where(w => w.Name == request.Name).OrderByDescending(w => w.Version)
					.Select(w => w.Version)
					.FirstOrDefaultAsync(cancellationToken);
			}, TimeSpan.FromHours(1));
		}
	}
}