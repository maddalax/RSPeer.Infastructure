using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Instance.Commands
{
	public class SaveClientInfoBulkCommand : IRequest<Unit>
	{
		public IEnumerable<RunescapeClient> Clients { get; set; }
	}
}