using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Application.Features.Store.Items.Queries;
using RSPeer.Common.Enums;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Purchase.Commands
{
    public class ModifyPricePerQuantityCommandHandler : IRequestHandler<ModifyPricePerQuantityCommand, Unit>
    {
        private readonly RsPeerContext _db;
        private readonly IMediator _mediator;

        public ModifyPricePerQuantityCommandHandler(RsPeerContext db, IMediator mediator)
        {
            _db = db;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(ModifyPricePerQuantityCommand request, CancellationToken cancellationToken)
        {
            var item = await _mediator.Send(new GetItemBySkuQuery {Sku = request.Sku}, cancellationToken);

            if (item == null)
            {
                throw new NotFoundException("Item", request.Sku);
            }

            AssertValidRequest(request, item);

            if (item.Type != ItemType.PremiumScript && !request.User.IsOwner)
            {
                throw new AuthorizationException("Permission denied.");
            }

            if (!request.User.IsOwner)
            {
                await AssertValidScriptAccess(item, request, cancellationToken);
            }

            switch (request.Action)
            {
                case CrudAction.Create:
                {
                    var recordByQuantity = await _db.PricePerQuantity.FirstOrDefaultAsync(w =>
                        w.Sku == request.Sku && w.Quantity == request.Quantity, cancellationToken);

                    if (recordByQuantity != null)
                    {
                        throw new Exception("Record already exists for that quantity.");
                    }

                    await _db.PricePerQuantity.AddAsync(new PricePerQuantity
                    {
                        Sku = request.Sku,
                        Price = request.Price,
                        Quantity = request.Quantity
                    }, cancellationToken);

                    break;
                }

                case CrudAction.Delete:
                {
                    var record = await _db.PricePerQuantity.FirstOrDefaultAsync(w =>
                            w.Sku == request.Sku && w.Price == request.Price && w.Quantity == request.Quantity,
                        cancellationToken);


                    if (record == null)
                    {
                        return Unit.Value;
                    }

                    _db.Remove(record);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }


            await _db.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        private async Task AssertValidScriptAccess(Item item, ModifyPricePerQuantityCommand request,
            CancellationToken cancellationToken)
        {
            if (item.Type != ItemType.PremiumScript)
            {
                return;
            }

            var script = await _mediator.Send(new GetScriptByIdQuery
            {
                ScriptId = int.Parse(request.Sku.Replace("premium-script-", ""))
            }, cancellationToken);

            if (script == null)
            {
                throw new NotFoundException("Script", request.Sku);
            }

            if (script.AuthorId != request.User.Id)
            {
                throw new AuthenticationException("This is not your script.");
            }
        }

        private void AssertValidRequest(ModifyPricePerQuantityCommand request, Item item)
        {
            if (request.Price <= 0 || request.Quantity < 2)
            {
                throw new Exception("Price must be greater than 0. Quantity must be greater than 1.");
            }

            if (request.Price > item.Price)
            {
                throw new Exception("Price must not be greater than its default price of " + item.Price + " tokens.");
            }

            if (request.Price == item.Price)
            {
                throw new Exception("Price must not be the same as the default price.");
            }
        }
    }
}