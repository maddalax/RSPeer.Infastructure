using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Store.Process.Commands.Children;
using RSPeer.Application.Features.Store.Purchase.Models;
using RSPeer.Application.Features.UserManagement.Users.Commands;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Common.Enums;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Process.Commands
{
	public class ProcessTokenOrderCommandHandler : IRequestHandler<ProcessTokenOrderCommand, PurchaseItemResult>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;
		private readonly IRedisService _redis;
		private bool didSetProcessLock;

		private readonly HashSet<string> CheckProcessing = new HashSet<string>
		{
			"rs3-inuvation-access"
		};

		public ProcessTokenOrderCommandHandler(RsPeerContext db, IMediator mediator, IRedisService redis)
		{
			_db = db;
			_mediator = mediator;
			_redis = redis;
		}

		public async Task<PurchaseItemResult> Handle(ProcessTokenOrderCommand request,
			CancellationToken cancellationToken)
		{
			await AssertHasBalance(request);

			var redisKey = $"processing_order_{request.Item.Sku}_{request.User.Id}";

			if (CheckProcessing.Contains(request.Item.Sku))
			{
				var processing = await _redis.GetString(redisKey);
				if (processing != null)
				{
					throw new Exception("An order for this item is in process, please wait until this is finished.");
				}

				didSetProcessLock = true;
				await _redis.Set(redisKey, DateTimeOffset.UtcNow.ToString(),
					TimeSpan.FromMinutes(1));
			}

			using (var transaction = await _db.Database.BeginTransactionAsync(cancellationToken))
			{
				try
				{
					PurchaseItemResult result;
					if (request.Item.Sku.StartsWith("premium-script-"))
					{
						result = await _mediator.Send(new ProcessScriptPurchaseCommand
						{
							Item = request.Item,
							Order = request.Order
						}, cancellationToken);
					}

					else if (request.Item.Sku == "unlimitedInstances" || request.Item.Sku == "instances")
					{
						result = await _mediator.Send(new ProcessInstancePurchaseCommand
						{
							Item = request.Item,
							Order = request.Order
						}, cancellationToken);
					}

					else if (request.Item.Sku == "rs3-inuvation-access")
					{
						throw new Exception("Inuvation is disabled due to RuneScape shutting down their RS3 Java Client and may not be purchased.");
					}

					else
					{
						throw new Exception("Failed to find item to process for order. OrderId: " + request.Order.Id);
					}

					await RemoveBalance(request, result);

					transaction.Commit();

					return result ?? new PurchaseItemResult
					{
						PaymentMethod = PaymentMethod.Tokens,
						Sku = request.Item.Sku,
						Status = OrderStatus.Completed,
						Total = request.Order.Total
					};
				}
				finally
				{
					if (didSetProcessLock)
					{
						await _redis.Remove(redisKey);
					}
				}
			}
		}

		private async Task RemoveBalance(ProcessTokenOrderCommand command, PurchaseItemResult result)
		{
			//If they are the creator of what they are purchasing, such as a premium script, do not charge them.
			if (result != null && result.IsCreator)
			{
				return;
			}

			await _mediator.Send(new UserUpdateBalanceCommand
			{
				Amount = (int) command.Order.Total,
				OrderId = command.Order.Id,
				UserId = command.User.Id,
				Type = AddRemove.Remove
			});
		}

		private async Task AssertHasBalance(ProcessTokenOrderCommand request)
		{
			var user = await _db.Users.FirstOrDefaultAsync(w => w.Id == request.User.Id);
			if (user.Balance < request.Order.Total)
			{
				_db.Orders.Remove(request.Order);
				await _db.SaveChangesAsync();
				throw new Exception("Insufficient funds.");
			}
		}
	}
}