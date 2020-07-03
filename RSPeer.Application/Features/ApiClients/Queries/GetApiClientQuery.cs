using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.ApiClients.Queries
{
    public class GetApiClientQuery : IRequest<ApiClient>
    {
        public User User { get; set; }
    }
}