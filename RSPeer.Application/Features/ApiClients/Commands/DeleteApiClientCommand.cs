using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.ApiClients.Commands
{
    public class DeleteApiClientCommand : IRequest<Unit>
    {
        public User User { get; set; }
    }
}