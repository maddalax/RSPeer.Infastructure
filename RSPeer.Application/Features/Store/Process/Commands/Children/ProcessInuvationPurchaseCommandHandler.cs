using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RSPeer.Application.Features.Discord.Commands;
using RSPeer.Application.Features.Discord.Models;
using RSPeer.Application.Features.Store.Purchase.Models;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Domain.Constants;
using RSPeer.Domain.Entities;
using RSPeer.Domain.Exceptions;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Process.Commands.Children
{
    public class ProcessInuvationPurchaseCommandHandler : IRequestHandler<ProcessInuvationPurchaseCommand, PurchaseItemResult>
    {
        private readonly RsPeerContext _db;
        private readonly IMediator _mediator;
        private readonly int _userCap;

        public ProcessInuvationPurchaseCommandHandler(IConfiguration config, RsPeerContext db, IMediator mediator)
        {
            _userCap = config.GetValue<int>("Inuvation:UserCap");
            _db = db;
            _mediator = mediator;
        }

        public async Task<PurchaseItemResult> Handle(ProcessInuvationPurchaseCommand request,
            CancellationToken cancellationToken)
        {
            
            var release = new DateTime(2019, 8, 6, 10, 0, 0, DateTimeKind.Utc);
            var now = DateTime.UtcNow;

            if (release > now)
            {
                throw new UserException("Inuvation purchase will be opened on August 6th, 2019 at 10:00 A.M. UTC");
            }
            
            // If they already have access, then a new purchase will just extend it 30 days.
            var hasAccess = await _db.UserGroups.AnyAsync(w =>
                w.GroupId == GroupConstants.InuvationId && w.UserId == request.Order.UserId, cancellationToken);

            if (!hasAccess)
            {
                var count = await _db.UserGroups.CountAsync(w => w.GroupId == GroupConstants.InuvationId,
                    cancellationToken);

                if (count >= _userCap)
                {
                    request.Order.Status = OrderStatus.Failed;
                    throw new UserException(
                        $"RS3 Inuvation has reached its cap of {_userCap} users, please try again later.");
                }
            }

            request.Order.Status = OrderStatus.Completed;

            if (!hasAccess)
            {
                await _db.UserGroups.AddAsync(new UserGroup
                {
                    GroupId = GroupConstants.InuvationId,
                    UserId = request.Order.UserId
                }, cancellationToken);
            }
            
            var isPreRelease = now.Month == 8 && now.Year == 2019 && request.Order.Quantity == 1 && !hasAccess;

            if (isPreRelease)
            {
                await _db.Data.AddAsync(new Data
                {
                    Key = $"inuvation:prerelease:{request.Order.Id}",
                    Value = request.Order.UserId.ToString()
                }, cancellationToken);
            }

            _db.Orders.Update(request.Order);
            

            await _db.SaveChangesAsync(cancellationToken);

            var user = await _mediator.Send(new GetUserByIdQuery {Id = request.Order.UserId}, cancellationToken);
            
            await _mediator.Send(new SendDiscordWebHookCommand
            {
                Message = $"{user?.Username} has purchased Inuvation access.",
                Type = DiscordWebHookType.InuvationPurchase
            }, cancellationToken);

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