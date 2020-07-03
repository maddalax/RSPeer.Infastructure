using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Infrastructure.Cognito.Users.Commands;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class UserSignUpCommandHandler : IRequestHandler<UserSignUpCommand, int>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public UserSignUpCommandHandler(IMediator mediator, RsPeerContext db)
		{
			_mediator = mediator;
			_db = db;
		}

		public async Task<int> Handle(UserSignUpCommand request, CancellationToken cancellationToken)
		{
			request = CleanCommand(request);
			await AssertCanRegister(request);
			using (var transaction = await _db.Database.BeginTransactionAsync(cancellationToken))
			{
				var user = new User
				{
					Username = request.Username,
					Email = request.Email,
					LinkKey = Guid.NewGuid()
				};

				await _db.Users.AddAsync(user, cancellationToken);
				await _db.SaveChangesAsync(cancellationToken);

				await _mediator.Send(new CognitoSignUpCommand
				{
					Email = request.Email,
					Password = request.Password,
					Username = request.Username
				}, cancellationToken);
				
				transaction.Commit();

				return user.Id;
			}
		}

		private UserSignUpCommand CleanCommand(UserSignUpCommand command)
		{
			command.Email = command.Email.ToLower().Trim();
			command.Username = command.Username.Trim();
			return command;
		}

		private async Task AssertCanRegister(UserSignUpCommand command)
		{
			// ReSharper disable once SpecifyStringComparison
			if (await _db.Users.AnyAsync(w => w.Username.ToLower() == command.Username.ToLower()))
			{
				throw new Exception("Username is already taken.");
			}
			// ReSharper disable once SpecifyStringComparison
			if (await _db.Users.AnyAsync(w => w.Email.ToLower() == command.Email.ToLower()))
			{
				throw new Exception("Email is already taken.");
			}
		}
	}
}