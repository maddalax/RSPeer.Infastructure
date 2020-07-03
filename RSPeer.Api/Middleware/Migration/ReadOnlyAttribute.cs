using Microsoft.AspNetCore.Mvc;

namespace RSPeer.Api.Middleware.Migration
{
	public class ReadOnlyAttribute : TypeFilterAttribute
	{
		public ReadOnlyAttribute() : base(typeof(ReadOnlyFilter))
		{
		}
	}
}