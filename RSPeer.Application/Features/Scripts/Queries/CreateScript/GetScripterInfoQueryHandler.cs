using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripts.Queries.CreateScript
{
	public class GetScripterInfoQueryHandler : IRequestHandler<GetScripterInfoQuery, ScripterInfo>
	{
		private readonly RsPeerContext _db;

		public GetScripterInfoQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<ScripterInfo> Handle(GetScripterInfoQuery request, CancellationToken cancellationToken)
		{
			return await _db.ScripterInfo
				.FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);
		}
	}
}