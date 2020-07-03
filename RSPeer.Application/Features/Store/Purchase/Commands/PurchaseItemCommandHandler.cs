using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.Store.Items.Queries;
using RSPeer.Application.Features.Store.Paypal.Commands;
using RSPeer.Application.Features.Store.Process.Commands;
using RSPeer.Application.Features.Store.Purchase.Models;
using RSPeer.Application.Features.Store.Purchase.Queries;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Purchase.Commands
{
	public class PurchaseItemCommandHandler : IRequestHandler<PurchaseItemCommand, PurchaseItemResult>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;
		private readonly IConfiguration _config;

		public PurchaseItemCommandHandler(IMediator mediator, RsPeerContext db, IConfiguration config)
		{
			_mediator = mediator;
			_db = db;
			_config = config;
		}

		public async Task<PurchaseItemResult> Handle(PurchaseItemCommand request, CancellationToken cancellationToken)
		{
			if (request.Quantity <= 0)
			{
				throw new Exception("Quantity must be greater than 0.");
			}
			
			var item = await _mediator.Send(new GetItemBySkuQuery { Sku = request.Sku }, cancellationToken);

			if (item == null)
			{
				throw new NotFoundException("Item", request.Sku);
			}

			if (item.Type == ItemType.Tokens)
			{
				AssertValidTokenAmount(request);
			}
			
			var price = item.Type == ItemType.PremiumScript
				? await _mediator.Send(new GetCustomPriceForItemQuery {Quantity = request.Quantity, Item = item},
					cancellationToken)
				: item.Price;

			var total = price * request.Quantity;

			if (item.PaymentMethod == PaymentMethod.Paypal)
			{
				var result = await _mediator.Send(new PaypalCreateOrderCommand
				{
					Description = item.Description,
					Name = item.Name,
					Quantity = request.Quantity,
					RedirectUrlCancel = _config.GetValue<string>("Paypal:RedirectCancel"),
					RedirectUrlSuccess = _config.GetValue<string>("Paypal:RedirectSuccess"),
					Sku = item.Sku,
					Total = total,
					User = request.User
				}, cancellationToken);

				await CreateOrder(request.User, item, total, request.Quantity, request.Recurring, result.PaypalId);
				return BuildResult(item, OrderStatus.Created, result.Url, total);
			}

			var order = await CreateOrder(request.User, item, total, request.Quantity, request.Recurring);
			return await _mediator.Send(new ProcessTokenOrderCommand
			{
				Order = order,
				Item = item,
				User = request.User
			}, cancellationToken);
		}

		private async Task<Order> CreateOrder(User user, Item item, decimal total, int quantity, bool recurring, string paypalId = null)
		{
			var order = new Order
			{
				ItemId = item.Id,
				Status = OrderStatus.Created,
				Timestamp = DateTimeOffset.UtcNow,
				Total = total,
				PaypalId = paypalId,
				UserId = user.Id,
				Quantity = quantity,
				Recurring = recurring
			};
			_db.Orders.Add(order);
			await _db.SaveChangesAsync();
			return order;
		}

		private void AssertValidTokenAmount(PurchaseItemCommand command)
		{
			var allowed = new[] {500, 1000, 1500, 2000, 2500, 5000, 7500, 20000};
			if (!allowed.Contains(command.Quantity))
			{
				throw new Exception("Invalid quantity, must be in " + JsonSerializer.Serialize(allowed));
			}
		}

		private PurchaseItemResult BuildResult(Item item, OrderStatus status, string meta, decimal total)
		{
			return new PurchaseItemResult
			{
				Meta = meta,
				PaymentMethod = item.PaymentMethod,
				Sku = item.Sku,
				Status = status,
				Total = total
			};
		}
	}
}