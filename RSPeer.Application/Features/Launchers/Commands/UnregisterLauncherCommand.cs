using System;
using MediatR;

namespace RSPeer.Application.Features.Launchers.Commands
{
    public class UnregisterLauncherCommand : IRequest<Unit>
    {
        public int UserId { get; set; }
        public Guid Tag { get; set; }
    }
}