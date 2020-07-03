using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Infrastructure.Cognito.Users.Commands;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class UserResetPasswordConfirmCommandHandler : IRequestHandler<UserResetPasswordConfirmCommand, Unit>
	{
		private readonly IMediator _mediator;

		public UserResetPasswordConfirmCommandHandler(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<Unit> Handle(UserResetPasswordConfirmCommand request, CancellationToken cancellationToken)
		{
			await _mediator.Send(new CognitoForgotPasswordConfirmCommand
			{
				Code = request.Code,
				Email = request.Email,
				Password = request.Password
			}, cancellationToken);
			return Unit.Value;
		}
	}
}