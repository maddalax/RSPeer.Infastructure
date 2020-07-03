using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.ScriptAccesses.Queries
{
	public class HasPrivateScriptAccessQueryHandler : IRequestHandler<HasPrivateScriptAccessQuery, bool>
	{
		private readonly RsPeerContext _db;

		public HasPrivateScriptAccessQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<bool> Handle(HasPrivateScriptAccessQuery request, CancellationToken cancellationToken)
		{
			return await _db.PrivateScriptAccess.AnyAsync(w =>
				w.ScriptId == request.ScriptId && w.UserId == request.UserId, cancellationToken: cancellationToken);
		}
	}
}