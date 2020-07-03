using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Migration.Queries;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Api.Jobs.Children
{
	public class SetTrueScriptCounts
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public SetTrueScriptCounts(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task Execute()
		{
			var enabled = await _mediator.Send(new IsReadOnlyModeQuery());
			
			if (enabled)
			{
				return;
			}
			
			var date = DateTimeOffset.UtcNow;
			var scripts = await _db.Scripts.ToListAsync();
			foreach (var s in scripts)
			{
				var query = _db.ScriptAccess.Where(w => w.ScriptId == s.Id);
				if (s.Type == ScriptType.Premium)
				{
					query = query.Where(w => w.Expiration.HasValue && w.Expiration.Value > date);
				}
				var count = await query.CountAsync();
				s.TotalUsers = count;
				_db.Scripts.Update(s);
			}
			await _db.SaveChangesAsync();
		}
	}
}