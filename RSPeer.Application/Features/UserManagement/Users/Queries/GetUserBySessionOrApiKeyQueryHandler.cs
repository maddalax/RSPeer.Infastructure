using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.ApiClients.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
    public class GetUserBySessionOrApiKeyQueryHandler : IRequestHandler<GetUserBySessionOrApiKeyQuery, User>
    {
        private readonly IMediator _mediator;

        public GetUserBySessionOrApiKeyQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<User> Handle(GetUserBySessionOrApiKeyQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Session))
            {
                return null;
            }
            
            // JWT's have . in them. Api Client does not.
            if (request.Session.Contains("."))
            {
                var user = await _mediator.Send(new GetUserByTokenQuery {Token = request.Session, Options = request.Options}, cancellationToken);

                if (user != null)
                {
                    return user;
                }    
            }
            
            var apiClient = await _mediator.Send(new GetApiClientByKeyQuery { Key = request.Session }, cancellationToken);
            
            if (apiClient == null)
            {
                return null;
            }

            return await _mediator.Send(new GetUserByIdQuery
            {
                Id = apiClient.UserId, IncludeGroups = request.Options?.IncludeGroups ?? false, AllowCached = request.Options?.
                    AllowCached ?? true, IncludeInstances = request.Options?.FullUser ?? false
            }, cancellationToken);
        }
    }
}