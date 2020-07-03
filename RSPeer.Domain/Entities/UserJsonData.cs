using System.ComponentModel.DataAnnotations.Schema;

namespace RSPeer.Domain.Entities
{
	public class UserJsonData
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int UserId { get; set; }
		public string Key { get; set; }
		[Column(TypeName = "jsonb")] public string Value { get; set; }
	}
}