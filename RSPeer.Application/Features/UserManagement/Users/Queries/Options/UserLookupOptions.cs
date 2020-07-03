namespace RSPeer.Application.Features.UserManagement.Users.Queries.Options
{
	public class UserLookupOptions
	{
		public bool IncludeBalance { get; set; }
		public bool IncludeGroups { get; set; }
		
		public bool AllowCached { get; set; }
		
		public bool FullUser { get; set; }
	}
}