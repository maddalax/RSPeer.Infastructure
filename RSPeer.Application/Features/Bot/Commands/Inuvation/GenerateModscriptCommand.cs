using MediatR;

namespace RSPeer.Application.Features.Bot.Commands.Inuvation
{
    public class GenerateModscriptCommand : IRequest<byte[]>
    {
        public long Hash { get; set; }
        public string Sha1 { get; set; }
        public string Archive { get; set; }
        public string Secret { get; set; }
        public string Vector { get; set; }
        public int UserId { get; set; }
    }
}