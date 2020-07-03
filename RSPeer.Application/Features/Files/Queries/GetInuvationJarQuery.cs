using MediatR;

namespace RSPeer.Application.Features.Files.Queries
{
    public class GetInuvationJarQuery : IRequest<byte[]>
    {
        public int UserId { get; set; }
    }
}