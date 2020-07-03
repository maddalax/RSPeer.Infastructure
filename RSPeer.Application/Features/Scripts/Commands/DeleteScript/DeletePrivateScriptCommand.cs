using MediatR;

namespace RSPeer.Application.Features.Scripts.Commands.DeleteScript
{
    public class DeletePrivateScriptCommand : IRequest<Unit>
    {
        public int ScriptId { get; set; }
        public int UserId { get; set; }
    }
}