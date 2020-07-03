using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.ScriptAccesses.Queries;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.ScriptAccesses.Commands
{
	public class AddScriptAccessCommandHandler : IRequestHandler<AddScriptAccessCommand, Unit>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public AddScriptAccessCommandHandler(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<Unit> Handle(AddScriptAccessCommand request, CancellationToken cancellationToken)
		{
			var script = await _mediator.Send(new GetFullScriptByIdQuery { ScriptId = request.ScriptId }, cancellationToken);

			if (script.Type == ScriptType.Premium && !request.AdminUserId.HasValue)
			{
				throw new AuthorizationException(
					"Premium script access must have an order id, unless an administrator is adding.");
			}

			if (script.Type == ScriptType.Premium && request.AdminUserId.HasValue)
			{
				var admin = await _mediator.Send(
					new GetUserByIdQuery { AllowCached = false, IncludeGroups = true, Id = request.AdminUserId.Value },
					cancellationToken);

				if (!admin.IsOwner)
				{
					throw new Exception("User specified for administrator is not an owner.");
				}
			}

			if (script.Type == ScriptType.Private)
			{
				var hasAccess = await _mediator.Send(new HasPrivateScriptAccessQuery
					{ UserId = request.UserId, ScriptId = script.Id }, cancellationToken);

				if (!hasAccess)
				{
					throw new AuthorizationException("You do not have access to this script.");
				}
			}

			if (script.Type == ScriptType.Free || script.Type == ScriptType.Premium && script.UserId == request.UserId)
			{
				if (await _db.ScriptAccess.AnyAsync(w => w.ScriptId == script.Id && w.UserId == request.UserId, cancellationToken))
				{
					throw new Exception("You already have this script added!");
				}
			}

			await _db.ScriptAccess.AddAsync(new ScriptAccess
			{
				ScriptId = script.Id,
				UserId = request.UserId,
				AdminUserId = request.AdminUserId,
				Timestamp = DateTimeOffset.UtcNow,
				Expiration = script.Type == ScriptType.Premium
					? DateTimeOffset.UtcNow.AddDays(30)
					: (DateTimeOffset?) null,
				Instances = script.Instances,
				Recurring = false
			}, cancellationToken);

			script.TotalUsers = script.TotalUsers <= 0 ? 1 : script.TotalUsers + 1;
			await _mediator.Send(new ClearScriptAccessCacheCommand { UserId = request.UserId }, cancellationToken);
			_db.Scripts.Update(script);
			await _db.SaveChangesAsync(cancellationToken);
			return Unit.Value;
		}
	}
}