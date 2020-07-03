using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using RSPeer.Application.Features.Store.Paypal.Commands.Base;
using RSPeer.Application.Features.Store.Paypal.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RSPeer.Application.Features.Store.Paypal.Commands
{
    public class PaypalGetWebHooksCommandHandler : BasePaypalCommandHandler<PaypalGetWebHooksCommand, PaypalWebhooksResult>
    { 
        private readonly string _baseUrl;

        public PaypalGetWebHooksCommandHandler(IHttpClientFactory factory, IMediator mediator, IConfiguration configuration) : base(factory, mediator)
        {
            _baseUrl = configuration.GetValue<string>("Paypal:BaseUrl");

        }
        
        public override async Task<PaypalWebhooksResult> Handle(PaypalGetWebHooksCommand request, CancellationToken cancellationToken)
        {
            var events = await MakeAuthorizedRequest(HttpMethod.Get, _baseUrl, "notifications/webhooks-events?page_size=50");
            return JsonSerializer.Deserialize<PaypalWebhooksResult>(events);
        }
    }
}