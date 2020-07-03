using MediatR;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
    public class GetUserIdBySessionQuery : IRequest<int?>
    {
        public bool AllowCached { get; set; } = true;
        public string Session { get; set; }
    }
}