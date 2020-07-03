using MediatR;
using Microsoft.AspNetCore.Http;
using RSPeer.Api.Utilities;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Common.Extensions;
using Sentry.AspNetCore;
using Sentry.Protocol;

namespace RSPeer.Api.Activators
{
    public class SentryUserFactory : IUserFactory
    {
        private readonly IMediator _mediator;

        public SentryUserFactory(IMediator mediator)
        {
            _mediator = mediator;
        }

        public User Create(HttpContext context)
        {
            var session = HttpUtilities.TryGetSession(context);
            if (session == null)
            {
                return null;
            }

            var userId = AsyncExtensions.RunSync(() => _mediator.Send(new GetUserIdBySessionQuery {Session = session}));
            
            if (!userId.HasValue)
            {
                return null;
            }

            return new User
            {
                Id = userId.ToString()
            };
        }
    }
}