using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Nelibur.ObjectMapper;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.Scripts.Commands.CompileScript;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Application.Features.Store.Purchase.Queries;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripts.Commands.CreateScript
{
	public class CreateScriptCommandHandler : IRequestHandler<CreateScriptCommand, int>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public CreateScriptCommandHandler(IMediator mediator, RsPeerContext db)
		{
			_mediator = mediator;
			_db = db;
		}

		public async Task<int> Handle(CreateScriptCommand request, CancellationToken cancellationToken)
		{
			request = CleanCommand(request);
			using (var transaction = await _db.Database.BeginTransactionAsync(cancellationToken))
			{
				var script = request.Script;

				var exists =
					await _db.Scripts.AsQueryable().Where(w => w.Name.ToLower() == script.Name.ToLower() || w.Id == script.Id)
						.OrderByDescending(w => w.Id).FirstOrDefaultAsync(cancellationToken);
				
				var isUpdatingPending =
					exists != null && (exists.Status == ScriptStatus.Pending || exists.Status == ScriptStatus.Denied);
				
				await AssertCanCreate(request, exists, isUpdatingPending);
			
				if(exists == null) 
				{
					script.DateAdded = DateTimeOffset.UtcNow;
				}

				if (exists != null && exists.Game != request.Script.Game)
				{
					throw new Exception("You may not switch which game the script is for.");
				}
				
				if (script.Type == ScriptType.Free && script.Price.HasValue && script.Price.Value > 0)
				{
					throw new Exception("You may not set a price on free scripts.");
				}
				
				if (script.Type == ScriptType.Free && script.Instances.HasValue && script.Instances.Value > 0)
				{
					throw new Exception("You may not set an instance limit on free scripts.");
				}

				// Price change.
				if (exists != null && exists.Type == ScriptType.Premium && script.Price.GetValueOrDefault(0) != exists.Price.GetValueOrDefault(0))
				{
					var prices = await _mediator.Send(new GetCustomPricesForItemQuery {Sku = $"premium-script-{exists.Id}"}, cancellationToken);
					
					if (prices.Any())
					{
						throw new Exception("You may not change script price when you have custom pricing discounts, please remove all discounts.");
					}
				}
				
				script.UserId = request.User.Id;
				script.RepositoryUrl = request.Script.RepositoryUrl;
				script.Status = ScriptStatus.Pending;
				if (request.Admin == null || request.Admin != null && script.Id == request.Admin.Id)
				{
					script.LastUpdate = DateTimeOffset.UtcNow;
					script.Version = exists?.Version + (decimal) 0.01 ?? (decimal) 0.01;
				}
			
				if (isUpdatingPending)
				{
					var pending = await _db.PendingScripts.Where(w => w.PendingScriptId == exists.Id)
						.FirstOrDefaultAsync(cancellationToken: cancellationToken);

					if (pending != null)
					{
						pending.Status = ScriptStatus.Pending;
						pending.Message = null;
						_db.PendingScripts.Update(pending);
					}
				}
				
				if (isUpdatingPending)
				{
					exists = TinyMapper.Map(script, exists);
					_db.Scripts.Update(exists);
				}
				else
				{
					script.Id = 0;
					await _db.Scripts.AddAsync(script, cancellationToken);
				}

				await _db.SaveChangesAsync(cancellationToken);

				if (exists != null && !isUpdatingPending)
				{
					await _db.PendingScripts.AddAsync(new PendingScriptMap
					{
						LiveScriptId = exists.Id,
						PendingScriptId = script.Id
					}, cancellationToken);
				}

				CompiledScript compiledScript = null;
				if (request.Admin == null || request.Recompile)
				{
					compiledScript =
						await _mediator.Send(
							new CompileScriptCommand { GitlabUrl = request.Script.RepositoryUrl, Game = request.Script.Game },
							cancellationToken);
				}
	
				if (isUpdatingPending)
				{
					var content = await _mediator.Send(new GetScriptContentQuery { ScriptId = exists.Id },
						cancellationToken);
					
					if (compiledScript != null)
					{
						content.Content = compiledScript.Content;
					}
					
					_db.ScriptContents.Update(content);
				}
				else
				{
					if (compiledScript != null)
					{
						await _db.ScriptContents.AddAsync(new ScriptContent
						{
							ScriptId = script.Id,
							Content = compiledScript.Content
						}, cancellationToken);
					}
				}

				if (exists == null)
				{
					await _db.PendingScripts.AddAsync(new PendingScriptMap
					{
						LiveScriptId = script.Id,
						PendingScriptId = script.Id
					}, cancellationToken);
				}
				
				await _db.SaveChangesAsync(cancellationToken);

				transaction.Commit();

				return isUpdatingPending ? exists.Id : script.Id;
			}
		}

		private CreateScriptCommand CleanCommand(CreateScriptCommand command)
		{
			command.Script.Name = command.Script.Name.Trim();
			command.Script.Description = command.Script.Description.Trim();
			command.Script.TotalUsers = 0;
			command.Script.ScriptContent = null;
			return command;
		}

		private async Task AssertCanCreate(CreateScriptCommand request, Script current, bool isUpdatingPending)
		{
			if (current != null && current.UserId != request.User.Id)
				throw new Exception("Script already exists by that name.");

			var toCompare = request.Script;

			var byName = await _db.Scripts.Where(w => w.Name.ToLower().Trim() == request.Script.Name.ToLower().Trim()).ToListAsync();

			if (!isUpdatingPending && byName != null && byName.Count > 0 && byName.All(w => w.Id != request.Script.Id))
			{
				throw new Exception(
					"You already have a script by this name, did you mean to update that script instead? If so, update it from the modify page.");
			}

			var pending = await _db.PendingScripts.FirstOrDefaultAsync(w => w.PendingScriptId == request.Script.Id);
			
			if (isUpdatingPending && byName != null && byName.Count > 0)
			{
				if (pending == null)
				{
					throw new NotFoundException("Pending Script Record", request.Script.Id);
				}
				
				var liveScript = await _db.Scripts.FirstOrDefaultAsync(w => w.Id == pending.LiveScriptId);

				if (liveScript == null)
				{
					throw new NotFoundException("Live Script", pending.LiveScriptId);
				}

				if (byName.All(w => w.Id != liveScript.Id) && byName.All(w => w.Id != pending.PendingScriptId))
				{
					throw new Exception(
						"You already have a script by this name, did you mean to update that script instead? If so, update it from the modify page.");
				}
				
			}
			
			var byRepo = await _db.Scripts.Where(w => w.RepositoryUrl == toCompare.RepositoryUrl).ToListAsync();

			if (byRepo != null && byRepo.Count > 0 && byRepo.All(w => w.Id != toCompare.Id))
			{
				throw new Exception(
					"A script already exists using this repository.");
			}
			
			var byForumUrl = await _db.Scripts.Where(w => w.ForumThread == toCompare.ForumThread).ToListAsync();
			
			if (byForumUrl != null && byForumUrl.Count > 0 && byForumUrl.All(w => w.ForumThread != toCompare.ForumThread))
			{
				throw new Exception(
					"A script already exists using this forum url.");
			}
		}
	}
}