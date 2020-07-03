using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Files.Commands;
using RSPeer.Application.Features.Files.Utility;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Bot.Commands.Inuvation
{
    public class UpdateInuvationUpdaterCommandHandler : IRequestHandler<UpdateInuvationUpdaterCommand, Unit>
    {
        private readonly IMediator _mediator;
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;

        public UpdateInuvationUpdaterCommandHandler(IMediator mediator, RsPeerContext db, IRedisService redis)
        {
            _mediator = mediator;
            _db = db;
            _redis = redis;
        }

        public async Task<Unit> Handle(UpdateInuvationUpdaterCommand request, CancellationToken cancellationToken)
        {
            var stream = request.File.OpenReadStream();
			
            var hash = stream.CalculateHash();
            
            var file = await _mediator.Send(new PutFileCommand
            {
                Name = FileHelper.GetFileName(Game.Rs3Updater),
                Stream = stream,
                Version = request.Version
            }, cancellationToken);

            await _db.Data.AddAsync(new Data
            {
                Key = $"invuvation-updater:version:hash:{file.Version}",
                Value = hash
            }, cancellationToken);
			
            await _db.SaveChangesAsync(cancellationToken);
			
            await _redis.Set("latest_file_version_inuvation-updater.jar", request.Version);
			
            return Unit.Value;
        }
    }
}