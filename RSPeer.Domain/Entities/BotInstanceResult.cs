namespace RSPeer.Domain.Entities
{
	public class BotInstanceResult
	{
		public bool CanRunMore { get; set; }
		public long NewCount { get; set; }
		public long Running { get; set; }
		public int Allowed { get; set; }
	}
}