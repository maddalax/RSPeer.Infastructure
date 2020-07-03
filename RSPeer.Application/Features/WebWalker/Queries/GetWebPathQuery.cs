using MediatR;
using Newtonsoft.Json.Linq;

namespace RSPeer.Application.Features.WebWalker.Queries
{
    public class GetWebPathQuery : IRequest<GetWebPathResult>
    {
        public int UserId { get; set; }
        public WebPathType Type { get; set; }
        public JObject Payload { get; set; }

        public Domain.Entities.WebWalker? WebWalker { get; set; }
    }

    public enum WebPathType
    {
        Normal,
        Bank
    }

}