using MediatR;
using RSPeer.Application.Features.Store.Paypal.Models;

namespace RSPeer.Application.Features.Store.Paypal.Commands
{
    public class PaypalGetWebHooksCommand : IRequest<PaypalWebhooksResult>
    {
        
    }
}