using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.Groups.Queries;
using RSPeer.Domain.Constants;

namespace RSPeer.Application.Features.Files.Queries
{
    public class GetInuvationJarQueryHandler : IRequestHandler<GetInuvationJarQuery, byte[]>
    {
        private readonly IMediator _mediator;
        

        public GetInuvationJarQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<byte[]> Handle(GetInuvationJarQuery request, CancellationToken cancellationToken)
        {
            var hasAccess = await _mediator.Send(new HasGroupQuery(request.UserId, GroupConstants.InuvationId, 
                GroupConstants.InuvationMaintainerId), cancellationToken);
           
            if (!hasAccess)
            {
                throw new AuthorizationException("You do not have access to inuvation.");
            }

            return await _mediator.Send(new GetFileQuery {Name = "inuvation.jar"}, cancellationToken);
        }
    }
}