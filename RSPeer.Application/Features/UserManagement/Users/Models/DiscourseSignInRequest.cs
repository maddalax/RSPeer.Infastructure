using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserManagement.Users.Models
{
    public class DiscourseSignInRequest
    {
        public string Nonce { get; set; }
        public User User { get; set; }
        public string Redirect { get; set; }
    }
}