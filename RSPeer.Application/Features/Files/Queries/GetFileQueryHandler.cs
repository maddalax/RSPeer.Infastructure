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
	public class GetFileQueryHandler : IRequestHandler<GetFileQuery, byte[]>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;

		public GetFileQueryHandler(RsPeerContext db, IRedisService redis)
		{
			_db = db;
			_redis = redis;
		}

		public async Task<byte[]> Handle(GetFileQuery request, CancellationToken cancellationToken)
		{
			var key = !request.Version.HasValue ? $"file_{request.Name}_latest" 
				: $"file_{request.Name}_version_{request.Version.Value}";

			var value = await _redis.GetDatabase().StringGetAsync(key);

			if (value.HasValue)
			{
				return (byte[]) value;
			}
			
			var query = _db.Files.AsQueryable();

			if (request.Version.HasValue)
			{
				query = query.Where(w => w.Version == request.Version.Value);
			}
			
			var file = await query
				.Where(w => w.Name == request.Name)
				.OrderByDescending(w => w.Version)
				.Select(w => w.File)
				.FirstOrDefaultAsync(cancellationToken);
			
			await _redis.GetDatabase().StringSetAsync(key, file, TimeSpan.FromDays(30));
			return file;
		}
	}
}