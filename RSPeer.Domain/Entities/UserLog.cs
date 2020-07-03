using System;

namespace RSPeer.Domain.Entities
{
    public class UserLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        
        public DateTimeOffset Timestamp { get; set; }
    }
}