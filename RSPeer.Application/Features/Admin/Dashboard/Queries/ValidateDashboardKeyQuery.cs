using MediatR;

namespace RSPeer.Application.Features.Admin.Dashboard.Queries
{
    public class ValidateDashboardKeyQuery : IRequest<Unit>
    {
        public int UserId { get; set; }
        public string Key { get; set; }
    }
}