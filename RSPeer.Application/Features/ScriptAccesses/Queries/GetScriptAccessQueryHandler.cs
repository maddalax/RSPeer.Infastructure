using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.ScriptAccesses.Queries
{
	public class GetScriptAccessQueryHandler : IRequestHandler<GetScriptAccessQuery, IEnumerable<ScriptAccess>>
	{
		private readonly RsPeerContext _db;

		public GetScriptAccessQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<ScriptAccess>> Handle(GetScriptAccessQuery request, CancellationToken cancellationToken)
		{
			var queryable = _db.ScriptAccess.Where(w => w.UserId == request.UserId);
			if (request.IncludeScript)
			{
				queryable = queryable.Include(w => w.Script);
			}

			if (request.NonExpired)
			{
				var now = DateTimeOffset.UtcNow;
				queryable = queryable.Where(w => w.Expiration == null || w.Expiration > now);
			}

			return await queryable.OrderByDescending(w => w.Timestamp).ToListAsync(cancellationToken);
		}
	}
}