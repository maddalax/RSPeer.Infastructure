using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Infrastructure.Caching.Commands;
using RSPeer.Common.Enums;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.UserManagement.Users.Commands
{
	public class UserUpdateBalanceCommandHandler : IRequestHandler<UserUpdateBalanceCommand, int>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public UserUpdateBalanceCommandHandler(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<int> Handle(UserUpdateBalanceCommand request, CancellationToken cancellationToken)
		{
			if (request.Amount <= 0)
			{
				throw new Exception("Amount must be greater than 0.");
			}
			
			var user = await _db.Users.FirstOrDefaultAsync(w => w.Id == request.UserId, cancellationToken);
			
			if (user == null)
			{
				throw new UserNotFoundException(request.UserId);
			}

			var balanceChange = new BalanceChange
			{
				OldBalance = user.Balance
			};
		
			switch (request.Type)
			{
				case AddRemove.Add:
					user.Balance += request.Amount;
					break;
				case AddRemove.Remove:
					user.Balance -= request.Amount;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			balanceChange.NewBalance = user.Balance;
			balanceChange.UserId = user.Id;
			balanceChange.Timestamp = DateTimeOffset.UtcNow;
			balanceChange.AdminUserId = request.AdminUserId;
			balanceChange.OrderId = request.OrderId;
			balanceChange.Reason = request.Reason;

			_db.Users.Update(user);
			_db.BalanceChanges.Add(balanceChange);
			await _db.SaveChangesAsync(cancellationToken);
			await _mediator.Send(new UpdateUserCacheCommand(user.Username), cancellationToken);
			return user.Balance;
		}
	}
}