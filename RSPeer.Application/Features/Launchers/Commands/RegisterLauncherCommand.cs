using System;
using MediatR;

namespace RSPeer.Application.Features.Launchers.Commands
{
    public class RegisterLauncherCommand : IRequest<Unit>
    {
        public int? UserId { get; set; }
        public Guid Tag { get; set; }
        public string Ip { get; set; }
        public Guid? LinkKey { get; set; }
        
        public string Platform { get; set; }
        
        public string MachineUsername { get; set; }
        
        public string Host { get; set; }
    }
}