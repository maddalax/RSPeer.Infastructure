using System;

namespace RSPeer.Domain.Entities
{
    public class RemoteMessage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Consumer { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}