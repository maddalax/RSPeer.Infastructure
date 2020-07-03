using System;

namespace RSPeer.Domain.Entities
{
	public class BinaryFile
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public byte[] File { get; set; }

		public decimal Version { get; set; }
		
		public DateTimeOffset LastUpdate { get; set; }
	}
}