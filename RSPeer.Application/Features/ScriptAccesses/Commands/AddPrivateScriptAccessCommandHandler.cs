using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.ScriptAccesses.Commands
{
	public class AddPrivateScriptAccessCommandHandler : IRequestHandler<AddPrivateScriptAccessCommand, Unit>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public AddPrivateScriptAccessCommandHandler(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<Unit> Handle(AddPrivateScriptAccessCommand request, CancellationToken cancellationToken)
		{
			if (request.UserId == 0 && string.IsNullOrWhiteSpace(request.Username))
			{
				throw new Exception("No user id or username was specified, unable to add access.");
			}

			if (request.UserId == 0)
			{
				var user = await _mediator.Send(new GetUserByUsernameQuery { Username = request.Username }, cancellationToken);
				if (user == null)
				{
					throw new NotFoundException("User", request.Username);
				}

				request.UserId = user.Id;
			}
			
			var script = await _mediator.Send(new GetScriptByIdQuery { ScriptId = request.ScriptId }, cancellationToken);

			if (script.AuthorId != request.RequestingUserId && script.AuthorId != request.UserId)
			{
				throw new Exception("Only the developer of this script may add access.");
			}
			
			if (await _db.PrivateScriptAccess.AnyAsync(
				w => w.ScriptId == request.ScriptId && w.UserId == request.UserId, cancellationToken: cancellationToken))
			{
				return Unit.Value;
			}

			var access = new PrivateScriptAccess
			{
				ScriptId = request.ScriptId,
				UserId = request.UserId,
				Timestamp = DateTimeOffset.UtcNow
			};

			await _db.PrivateScriptAccess.AddAsync(access, cancellationToken);
			await _db.SaveChangesAsync(cancellationToken);
			return Unit.Value;
		}
	}
}