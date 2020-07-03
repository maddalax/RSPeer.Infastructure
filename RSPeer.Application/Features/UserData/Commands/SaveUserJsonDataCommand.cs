using MediatR;

namespace RSPeer.Application.Features.UserData.Commands
{
	public class SaveUserJsonDataCommand : IRequest<Unit>
	{
		public int UserId { get; set; }
		public string Key { get; set; }
		public object Value { get; set; }
	}
}