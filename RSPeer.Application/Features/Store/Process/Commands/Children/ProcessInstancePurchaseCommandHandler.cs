using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Store.Items.Queries;
using RSPeer.Application.Features.Store.Purchase.Models;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Process.Commands.Children
{
	public class ProcessInstancePurchaseCommandHandler : IRequestHandler<ProcessInstancePurchaseCommand, PurchaseItemResult>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;
		private readonly IMediator _mediator;

		public ProcessInstancePurchaseCommandHandler(RsPeerContext db, IRedisService redis, IMediator mediator)
		{
			_db = db;
			_redis = redis;
			_mediator = mediator;
		}

		public async Task<PurchaseItemResult> Handle(ProcessInstancePurchaseCommand request, CancellationToken cancellationToken)
		{
			var instances = await _mediator.Send(new GetItemBySkuQuery { Sku = "instances"}, cancellationToken);
			if (request.Item.Sku == "unlimitedInstances")
			{
				request.Order.Quantity = 1000000;
				request.Order.ItemId = instances.Id;
				request.Order.Item = instances;
			}
			request.Order.Status = OrderStatus.Completed;
			_db.Orders.Update(request.Order);
			await _db.SaveChangesAsync(cancellationToken);
			await _redis.Remove($"{request.Order.UserId}_instances_allowed");
			await _redis.Remove($"{request.Order.UserId}_instances_allowed_no_free");
			await _redis.Remove($"user_{request.Order.UserId}");

			return new PurchaseItemResult
			{
				IsCreator = false,
				PaymentMethod = PaymentMethod.Tokens,
				Sku = request.Item.Sku,
				Status = OrderStatus.Completed,
				Total = request.Order.Total
			};
		}
	}
}