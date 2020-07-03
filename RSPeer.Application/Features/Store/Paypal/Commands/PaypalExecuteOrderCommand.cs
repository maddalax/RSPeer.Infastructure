using MediatR;

namespace RSPeer.Application.Features.Store.Paypal.Commands
{
	public class PaypalExecuteOrderCommand : IRequest<Unit>
	{
		public string PaymentId { get; set; }
		public string PayerId { get; set; }
	}
}