using System;

namespace RSPeer.Domain.Entities
{
	public class ScripterInfo
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string GitlabUsername { get; set; }
		public long GitlabUserId { get; set; }
		public long GitlabGroupId { get; set; }
		public string GitlabGroupPath { get; set; }
		public DateTimeOffset DateAdded { get; set; }
	}
}