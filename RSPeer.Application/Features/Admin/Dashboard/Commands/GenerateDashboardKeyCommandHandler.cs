using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RSPeer.Application.Features.Discord.Commands;
using RSPeer.Application.Features.Discord.Models;
using RSPeer.Application.Features.Email.Commands;
using RSPeer.Application.Features.UserData.Commands;
using RSPeer.Common.Extensions;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Admin.Dashboard.Commands
{
    public class GenerateDashboardKeyCommandHandler : IRequestHandler<GenerateDashboardKeyCommand, Unit>
    {
        private readonly IMediator _mediator;
        private readonly RsPeerContext _db;

        public GenerateDashboardKeyCommandHandler(IMediator mediator, RsPeerContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        public async Task<Unit> Handle(GenerateDashboardKeyCommand request, CancellationToken cancellationToken)
        {
            var keys = await _db.UserJsonData.Where(w => w.Key == "dashboard_access_key" && w.UserId == request.UserId).ToListAsync(cancellationToken);
            _db.UserJsonData.RemoveRange(keys);
            await _db.SaveChangesAsync(cancellationToken);
            
            var expiration = DateTimeOffset.UtcNow.AddHours(8);
            var value = StringExtensions.GetUniqueKey(50);
            
            var key = new JObject 
                {{"value", value}, 
                {"expiration", expiration}};
            
            await _mediator.Send(new SaveUserJsonDataCommand {Key = "dashboard_access_key", 
                UserId = request.UserId, Value = key}, cancellationToken);
            
            await _mediator.Send(new SendEmailCommand
            {
                FromEmail = "noreply@rspeer.org",
                ToEmail = request.Email,
                Subject = "Dashboard Access Key",
                Body = $"Your access key is {value}"
            }, cancellationToken);

            await _mediator.Send(new SendDiscordWebHookCommand
            {
                Message = "Successfully sent dashboard access key to " + request.Email, Type = DiscordWebHookType.Log
            }, cancellationToken);
            
            return Unit.Value;
        }
    }
}