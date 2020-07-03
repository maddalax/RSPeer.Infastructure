using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripter.Payout.Queries
{
	public class GetScriptersToPayoutQuery : IRequest<IEnumerable<ScripterPayout>>
	{
		
	}
}