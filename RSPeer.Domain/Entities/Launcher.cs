using System;
using System.ComponentModel.DataAnnotations;

namespace RSPeer.Domain.Entities
{
    public class Launcher
    {
        [Key]
        public Guid Tag { get; set; }
        
        public string Ip { get; set; }
        
        public DateTimeOffset LastUpdate { get; set; }
        
        public int UserId { get; set; }
        
        public string Platform { get; set; }
        
        public string MachineUsername { get; set; }
        
        public string Host { get; set; }
    }
}