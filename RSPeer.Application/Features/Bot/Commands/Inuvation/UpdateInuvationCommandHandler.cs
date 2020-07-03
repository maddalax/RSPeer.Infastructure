using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Files.Commands;
using RSPeer.Application.Features.SiteConfig.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Bot.Commands.Inuvation
{
    public class UpdateInuvationCommandHandler : IRequestHandler<UpdateInuvationCommand, Unit>
    {
        private readonly IMediator _mediator;
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;

        public UpdateInuvationCommandHandler(IMediator mediator, RsPeerContext db, IRedisService redis)
        {
            _mediator = mediator;
            _db = db;
            _redis = redis;
        }

        public async Task<Unit> Handle(UpdateInuvationCommand request, CancellationToken cancellationToken)
        {
            var enabled = bool.Parse(await _mediator.Send(new GetSiteConfigOrThrowCommand {Key = "bot:update:enabled"}, 
                cancellationToken));

            if (!enabled)
            {
                throw new Exception("Bot update is disabled at this time.");
            }
			
            var stream = request.File.OpenReadStream();
			
            var hash = stream.CalculateHash();
			
            var file = await _mediator.Send(new PutFileCommand
            {
                Name = "inuvation.jar",
                Stream = stream,
                Version = request.Version
            }, cancellationToken);

            await _db.Data.AddAsync(new Data
            {
                Key = $"invuvation:version:hash:{file.Version}",
                Value = hash
            }, cancellationToken);
			
            await _db.SaveChangesAsync(cancellationToken);
			
            await _redis.Set("latest_file_version_inuvation.jar", request.Version);
			
            return Unit.Value;
        }
    }
}