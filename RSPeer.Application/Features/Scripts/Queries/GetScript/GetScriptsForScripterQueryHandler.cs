using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripts.Queries.GetScript
{
	public class GetScriptsForScripterQueryHandler : IRequestHandler<GetScriptsForScripterQuery, IEnumerable<Script>>
	{
		private readonly RsPeerContext _db;

		public GetScriptsForScripterQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<Script>> Handle(GetScriptsForScripterQuery request, CancellationToken cancellationToken)
		{
			return await _db.Scripts.Where(w => w.UserId == request.UserId).ToListAsync(cancellationToken);
		}
	}
}