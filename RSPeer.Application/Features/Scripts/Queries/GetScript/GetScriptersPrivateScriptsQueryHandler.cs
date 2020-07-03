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
	public class GetScriptersPrivateScriptsQueryHandler : IRequestHandler<GetScriptersPrivateScriptsQuery, IEnumerable<Script>>
	{
		private readonly RsPeerContext _db;

		public GetScriptersPrivateScriptsQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<Script>> Handle(GetScriptersPrivateScriptsQuery request, CancellationToken cancellationToken)
		{
			return await _db.Scripts.Where(w => w.Type == ScriptType.Private && w.UserId == request.UserId)
				.Include(w => w.PrivateScriptAccesses).ThenInclude(w => w.User)
				.ToListAsync(cancellationToken);
		}
	}
}