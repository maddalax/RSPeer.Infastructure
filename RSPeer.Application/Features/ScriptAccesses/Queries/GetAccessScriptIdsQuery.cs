using System.Collections.Generic;
using MediatR;

namespace RSPeer.Application.Features.ScriptAccesses.Queries
{
	public class GetAccessScriptIdsQuery : IRequest<IEnumerable<int>>
	{
		public int UserId { get; set; }
	}
}
