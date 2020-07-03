using System;

namespace RSPeer.Domain.Entities
{
    public class UserClientTag
    {
        public Guid Tag { get; set; }
        public int UserId { get; set; }
    }
}