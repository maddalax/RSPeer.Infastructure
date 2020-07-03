using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Store.Paypal.Queries;
using RSPeer.Application.Features.Store.Process.Commands;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Store.Paypal.Commands
{
	public class PaypalExecuteProcessingOrdersCommandHandler : IRequestHandler<PaypalExecuteProcessingOrdersCommand, Unit>
	{
		private readonly IMediator _mediator;

		public PaypalExecuteProcessingOrdersCommandHandler(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<Unit> Handle(PaypalExecuteProcessingOrdersCommand request,
			CancellationToken cancellationToken)
		{
			var orders = await _mediator.Send(new GetUnfinishedPaypalOrdersQuery(), cancellationToken);

			foreach (var order in orders)
			{
				if (order.Status == OrderStatus.Completed)
				{
					continue;
				}

				await _mediator.Send(new ProcessPaypalOrderCommand { Order = order }, cancellationToken);
			}
			
			return Unit.Value;
		}
	}
}