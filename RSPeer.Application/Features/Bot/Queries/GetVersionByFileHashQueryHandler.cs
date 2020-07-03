using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Bot.Queries
{
	public class GetVersionByFileHashQueryHandler : IRequestHandler<GetVersionByFileHashQuery, decimal>
	{
		private readonly RsPeerContext _db;

		public GetVersionByFileHashQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<decimal> Handle(GetVersionByFileHashQuery request, CancellationToken cancellationToken)
		{
			var key = request.Game == Game.Rs3 ? "invuvation:version:hash:" : "bot:version:hash:";
			var result = await _db.Data.FirstOrDefaultAsync(w => w.Value == request.Hash 
			                                                     && w.Key.Contains(key), 
				cancellationToken);

			return result == null 
				? default 
				: decimal.Parse(result.Key.Replace(key, string.Empty));
		}
	}
}