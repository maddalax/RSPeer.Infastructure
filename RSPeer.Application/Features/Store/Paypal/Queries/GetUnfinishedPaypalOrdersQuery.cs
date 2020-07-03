using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Store.Paypal.Queries
{
	public class GetUnfinishedPaypalOrdersQuery : IRequest<IEnumerable<Order>>
	{
	}
}