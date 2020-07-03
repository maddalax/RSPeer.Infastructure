using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSPeer.Domain.Entities.Discourse
{
    public class DiscourseUser
    {
        [Column("username")]
        public string Username { get; set; }
        
        [Column("id")]
        public int Id { get; set; }
        
        [Column("name")]
        public string Name { get; set; }
        
        [Column("created_at")]
        public DateTimeOffset Created_At { get; set; }
        
        [Column("updated_at")]
        public DateTimeOffset Updated_At { get; set; }

        [Column("username_lower")] 
        public string Username_Lower { get; set; }
        
        [Column("active")]
        public bool Active { get; set; }
        
        [Column("admin")]
        public bool Admin { get; set; }
        
        [Column("trust_level")]
        public int Trust_Level { get; set; }
    }
}