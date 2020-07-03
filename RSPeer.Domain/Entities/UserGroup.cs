using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RSPeer.Domain.Entities
{
	public class UserGroup
	{
		public int Id { get; set; }
		public int GroupId { get; set; }
		public int UserId { get; set; }

		[JsonIgnore] 
		[NotMapped]
		public User User { get; set; }

		[JsonIgnore]
		public Group Group { get; set; }
	}
}