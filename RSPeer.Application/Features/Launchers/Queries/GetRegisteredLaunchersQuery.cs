using System.Collections.Generic;
using MediatR;

namespace RSPeer.Application.Features.Launchers.Queries
{
    public class GetRegisteredLaunchersQuery : IRequest<IEnumerable<Domain.Entities.Launcher>>
    {
        public int UserId { get; set; }
    }
}