using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripts.Commands.DenyScript
{
    public class DenyScriptCommand : IRequest<Unit>
    {
        public User Admin { get; set; }
        public int ScriptId { get; set; }
        public string Reason { get; set; }
    }
}