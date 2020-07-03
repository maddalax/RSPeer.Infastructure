using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.ApiClients.Commands
{
    public class CreateApiClientCommand : IRequest<ApiClient>
    {
        public User User { get; set; }
    }
}