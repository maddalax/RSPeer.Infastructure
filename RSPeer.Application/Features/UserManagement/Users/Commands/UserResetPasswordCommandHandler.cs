using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Infrastructure.Cognito.Users.Commands;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class UserResetPasswordCommandHandler : IRequestHandler<UserResetPasswordCommand, Unit>
	{
		private readonly IMediator _mediator;

		public UserResetPasswordCommandHandler(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<Unit> Handle(UserResetPasswordCommand request, CancellationToken cancellationToken)
		{
			await _mediator.Send(new CognitoFormatPasswordCommand { Email = request.Email }, cancellationToken);
			return Unit.Value;
		}
	}
}