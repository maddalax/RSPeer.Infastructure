using System.Collections.Generic;
using MediatR;
using RSPeer.Application.Features.Stats.Models;

namespace RSPeer.Application.Features.Stats.Queries
{
	public class GetTop25UsersRunningQuery : IRequest<IEnumerable<UserAndCountResult>>
	{
		
	}
}