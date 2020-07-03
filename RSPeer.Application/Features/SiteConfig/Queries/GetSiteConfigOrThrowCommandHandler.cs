using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.SiteConfig.Queries
{
	public class GetSiteConfigOrThrowCommandHandler : IRequestHandler<GetSiteConfigOrThrowCommand, string>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;

		public GetSiteConfigOrThrowCommandHandler(RsPeerContext db, IRedisService redis)
		{
			_db = db;
			_redis = redis;
		}

		public async Task<string> Handle(GetSiteConfigOrThrowCommand request, CancellationToken cancellationToken)
		{
			var result = await _redis.GetOrDefault($"site_config_{request.Key}", async () =>
			{
				var config = await _db.SiteConfig.Where(w => w.Key == request.Key).FirstOrDefaultAsync(cancellationToken);
				if (config == null)
				{
					throw new NotFoundException("SiteConfig", request.Key);
				}

				return config.Value;
			});
			return result;
		}
	}
}