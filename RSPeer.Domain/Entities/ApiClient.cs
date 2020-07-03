using System;

namespace RSPeer.Domain.Entities
{
    public class ApiClient
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Key { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}