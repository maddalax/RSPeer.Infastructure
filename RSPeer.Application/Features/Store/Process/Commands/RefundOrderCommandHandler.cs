using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Application.Features.Store.Process.Queries;
using RSPeer.Application.Features.UserManagement.Users.Commands;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Common.Enums;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Process.Commands
{
	public class RefundOrderCommandHandler : IRequestHandler<RefundOrderCommand, Unit>
	{
		private readonly IMediator _mediator;
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;

		public RefundOrderCommandHandler(IMediator mediator, RsPeerContext db, IRedisService redis)
		{
			_mediator = mediator;
			_db = db;
			_redis = redis;
		}

		public async Task<Unit> Handle(RefundOrderCommand request, CancellationToken cancellationToken)
		{
			var order = await _mediator.Send(new GetOrderByIdQuery { OrderId = request.OrderId, IncludeItem = true, IsAdmin = true },
				cancellationToken);

			if (order == null)
			{
				throw new Exception("Failed to find order by id, can not refund.");
			}

			if (order.Status != OrderStatus.Completed)
			{
				throw new Exception("Order must be completed to refund.");
			}			
			
			if (order.IsRefunded)
			{
				throw new Exception("Order is already refunded.");
			}

			using (var transaction = await _db.Database.BeginTransactionAsync(cancellationToken))
			{
				order.IsRefunded = true;
				order.AdminUserId = request.AdminUserId;

				var toRefund = order.Total;
				if (order.Item.PaymentMethod == PaymentMethod.Paypal)
				{
					// If paypal, multiply their total by the token price, since we can only refund with tokens.
					toRefund /= (decimal) 0.01;
				}

				var user = await _mediator.Send(new GetUserByIdQuery { AllowCached = false, Id = order.UserId }, cancellationToken);

				if (order.Item.Type == ItemType.Tokens && user.Balance < order.Total)
				{
					throw new Exception("User has less tokens than the amount you are trying to refund.");
				}

				await _mediator.Send(new UserUpdateBalanceCommand
				{
					Type = order.Item.Type == ItemType.Tokens ? AddRemove.Remove : AddRemove.Add,
					UserId = order.UserId,
					AdminUserId = request.AdminUserId,
					Reason = $"order-refund-{request.OrderId}",
					Amount = (int) toRefund,
					OrderId = order.Id
				}, cancellationToken);

				if (order.Item.Type == ItemType.PremiumScript)
				{
					var scriptId = order.Item.Sku.Replace("premium-script-", string.Empty);
					var script = await _mediator.Send(new GetScriptByIdQuery { ScriptId = int.Parse(scriptId) },
						cancellationToken);

					if (script.AuthorId == order.UserId)
					{
						throw new Exception("You can not refund the developer for their own script.");
					}
					
					if (!order.Item.FeesPercent.HasValue)
					{
						throw new Exception(
							"Attempted to refund premium script, but fees percent did not have a value.");
					}

					var taken = order.Total * order.Item.FeesPercent.Value;
					var payout = order.Total - taken;

					var access = await _db.ScriptAccess.FirstOrDefaultAsync(w => w.OrderId.HasValue && w.OrderId.Value == order.Id, cancellationToken);

					if (access == null)
					{
						throw new Exception("Failed to find script access by that order.");
					}
					
					_db.ScriptAccess.Remove(access);

					await _mediator.Send(new UserUpdateBalanceCommand
					{
						Type = AddRemove.Remove,
						UserId = script.AuthorId,
						AdminUserId = request.AdminUserId,
						Reason = $"order-refund-{request.OrderId}",
						Amount = (int) payout,
						OrderId = order.Id
					}, cancellationToken);
				}

				_db.Orders.Update(order);
				await _redis.Remove($"{order.UserId}_instances_allowed");
				await _redis.Remove($"{order.UserId}_instances_allowed_no_free");
				await _redis.Remove($"user_{order.UserId}");
				await _db.SaveChangesAsync(cancellationToken);
				transaction.Commit();
			}

			return Unit.Value;
		}
	}
}