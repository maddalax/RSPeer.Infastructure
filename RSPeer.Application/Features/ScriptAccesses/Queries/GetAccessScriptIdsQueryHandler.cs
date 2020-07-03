using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.ScriptAccesses.Queries
{
	public class GetAccessScriptIdsQueryHandler : IRequestHandler<GetAccessScriptIdsQuery, IEnumerable<int>>
	{
		private readonly RsPeerContext _db;

		public GetAccessScriptIdsQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<int>> Handle(GetAccessScriptIdsQuery request, CancellationToken cancellationToken)
		{
			return await _db.ScriptAccess.Where(w => w.UserId == request.UserId).Select(w => w.ScriptId)
				.ToListAsync(cancellationToken);
		}
	}
}