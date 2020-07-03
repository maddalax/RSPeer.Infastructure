using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.ApiClients.Queries
{
    public class GetApiClientByKeyQuery : IRequest<ApiClient>
    {
        public string Key { get; set; }
    }
}