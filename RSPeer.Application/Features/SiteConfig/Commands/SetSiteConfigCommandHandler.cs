using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.SiteConfig.Commands
{
	public class SetSiteConfigCommandHandler : IRequestHandler<SetSiteConfigCommand, Unit>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;

		public SetSiteConfigCommandHandler(RsPeerContext db, IRedisService redis)
		{
			_db = db;
			_redis = redis;
		}

		public async Task<Unit> Handle(SetSiteConfigCommand request, CancellationToken cancellationToken)
		{
			var exists = await _db.SiteConfig.FirstOrDefaultAsync(w => w.Key == request.Key, cancellationToken: cancellationToken);

			if (exists != null)
			{
				exists.Value = request.Value;
				_db.SiteConfig.Update(exists);
			}
			else
			{
				var config = new Domain.Entities.SiteConfig
				{
					Key = request.Key,
					Value = request.Value
				};
				await _db.SiteConfig.AddAsync(config, cancellationToken);
			}

			await _db.SaveChangesAsync(cancellationToken);
			await _redis.SetJson($"site_config_{request.Key}", request.Value);
			return Unit.Value;
		}
	}
}