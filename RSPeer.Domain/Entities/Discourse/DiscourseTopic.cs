using System.ComponentModel.DataAnnotations.Schema;

namespace RSPeer.Domain.Entities.Discourse
{
    public class DiscourseTopic
    {
        [Column("title")]
        public string Title { get; set; }
        
        [Column("id")]
        public int Id { get; set; }
    }
}