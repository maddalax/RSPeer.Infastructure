using System;
using RSPeer.Domain.Dtos;

namespace RSPeer.Domain.Entities
{
	public class ScriptAccessDto
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public ScriptDto Script { get; set; }
		public int OrderId { get; set; }
		public DateTimeOffset Timestamp { get; set; }
		public DateTimeOffset? Expiration { get; set; }
		public int? Instances { get; set; }
		public bool Recurring { get; set; }
		
		public bool IsExpired { get; set; }

		public ScriptAccessDto(ScriptAccess access)
		{
			Id = access.Id;
			UserId = access.UserId;
			Script = new ScriptDto(access.Script);
			OrderId = access.OrderId ?? 0;
			Timestamp = access.Timestamp;
			Expiration = access.Expiration;
			Instances = access.Instances;
			Recurring = access.Recurring;
			var now = DateTimeOffset.UtcNow;
			IsExpired = Expiration.HasValue && Expiration.Value < now;
		}
	}
}