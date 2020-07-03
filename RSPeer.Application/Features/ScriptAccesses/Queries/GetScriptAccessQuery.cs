using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.ScriptAccesses.Queries
{
	public class GetScriptAccessQuery : IRequest<IEnumerable<ScriptAccess>>
	{
		public int UserId { get; set; }
		public bool IncludeScript { get; set; }
		public bool NonExpired { get; set; }
	}
}