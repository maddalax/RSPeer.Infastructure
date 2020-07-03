using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.Store.Paypal.Commands.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Paypal.Commands
{
	public class PaypalExecuteOrderCommandHandler : BasePaypalCommandHandler<PaypalExecuteOrderCommand, Unit>
	{
		private readonly RsPeerContext _db;
		private string _baseUrl;
		
		public PaypalExecuteOrderCommandHandler(IHttpClientFactory factory, IMediator mediator, RsPeerContext db, IConfiguration configuration) : base(factory, mediator)
		{
			_db = db;
			_baseUrl = configuration.GetValue<string>("Paypal:BaseUrl");
		}

		public override async Task<Unit> Handle(PaypalExecuteOrderCommand request, CancellationToken cancellationToken)
		{
			var order = await _db.Orders.FirstOrDefaultAsync(w => w.PaypalId == request.PaymentId, cancellationToken);
			if (order == null)
			{
				throw new NotFoundException("Paypal Order", request.PaymentId);
			}
			if (order.Status != OrderStatus.Created)
			{
				throw new Exception("Order is already processing or completed.");
			}
			var url = $"payments/payment/{order.PaypalId}/execute";
			var body = new Dictionary<string, string>
			{
				{ "payer_id", request.PayerId }
			};
			await using (var transaction = await _db.Database.BeginTransactionAsync(cancellationToken))
			{
				order.Status = OrderStatus.Processing;
				_db.Orders.Update(order);
				await _db.SaveChangesAsync(cancellationToken);
				await MakeAuthorizedPost(_baseUrl, url, body);
				transaction.Commit();
			}
			return Unit.Value;
		}
	}
}