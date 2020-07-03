using MediatR;

namespace RSPeer.Application.Features.UserData.Queries
{
	public class GetUserJsonDataQuery : IRequest<string>
	{
		public int UserId { get; set; }
		public string Key { get; set; }
	}
}