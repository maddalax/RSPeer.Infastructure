using MediatR;

namespace RSPeer.Application.Features.Admin.Dashboard.Commands
{
    public class GenerateDashboardKeyCommand : IRequest<Unit>
    {
        public int UserId { get; set; }
        public string Email { get; set; }
    }
}