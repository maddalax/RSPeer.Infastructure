using MediatR;
using Microsoft.AspNetCore.Http;

namespace RSPeer.Application.Features.Bot.Commands.Inuvation
{
    public class UpdateInuvationUpdaterCommand : IRequest<Unit>
    {
        public IFormFile File { get; set; }
        public decimal Version { get; set; }
    }
}