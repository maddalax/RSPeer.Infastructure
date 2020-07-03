using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.UserData.Queries
{
	public class GetUserJsonDataQueryHandler : IRequestHandler<GetUserJsonDataQuery, string>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;

		public GetUserJsonDataQueryHandler(RsPeerContext db, IRedisService redis)
		{
			_db = db;
			_redis = redis;
		}

		public async Task<string> Handle(GetUserJsonDataQuery request, CancellationToken cancellationToken)
		{
			var result = await _redis.GetOrDefault($"user_json_data_{request.Key}_{request.UserId}", async () =>
			{
				return await _db.UserJsonData.FirstOrDefaultAsync(w => w.UserId == request.UserId && w.Key == request.Key,
						cancellationToken);
			});
			return result?.Value;
		}
	}
}