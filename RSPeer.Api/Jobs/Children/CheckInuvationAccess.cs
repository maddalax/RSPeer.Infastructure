using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Discord.Commands;
using RSPeer.Application.Features.Store.Process.Queries;
using RSPeer.Domain.Constants;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Api.Jobs.Children
{
    public class CheckInuvationAccess
    {
        private readonly RsPeerContext _db;
        private readonly IMediator _mediator;

        public CheckInuvationAccess(RsPeerContext db, IMediator mediator)
        {
            _db = db;
            _mediator = mediator;
        }

        public async Task Execute()
        {
            /*
            var groups = await _db.UserGroups.Where(w => w.GroupId == GroupConstants.InuvationId).ToListAsync();
            foreach (var group in groups)
            {
                var expiration = await _mediator.Send(new GetOrdersExpirationQuery {Sku = "rs3-inuvation-access", UserId = group.UserId});
                
                if (expiration <= DateTimeOffset.Now)
                {
                    var message = "Your RSPeer Inuvation subscription has expired.";
                    await _db.UserLogs.AddAsync(new UserLog
                    {
                        Message = message,
                        Type = "rs3:subscription:expired",
                        Timestamp = DateTimeOffset.Now,
                        UserId = group.UserId
                    });
                    _db.UserGroups.Remove(group);
                    try
                    {
                        await _mediator.Send(new SendDiscordPrivateMessageCommand
                            {UserId = group.UserId, Message = message});
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            }

            await _db.SaveChangesAsync();
            */
        }
    }
}