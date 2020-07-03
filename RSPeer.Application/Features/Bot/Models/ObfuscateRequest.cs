using System.IO;

namespace RSPeer.Application.Features.Bot.Models
{
	public class ObfuscateRequest
	{
		public byte[] Bytes { get; set; }
		public string Config { get; set; }
		public Stream Stream { get; set; }
	}
}