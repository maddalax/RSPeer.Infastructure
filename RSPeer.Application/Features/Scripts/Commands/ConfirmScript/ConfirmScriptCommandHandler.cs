using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Nelibur.ObjectMapper;
using Newtonsoft.Json;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.Discord.Commands;
using RSPeer.Application.Features.Discord.Models;
using RSPeer.Application.Features.ScriptAccesses.Commands;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Application.Features.SiteConfig.Queries;
using RSPeer.Application.Features.Store.Items.Commands;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Dtos;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;
using StackExchange.Redis;

namespace RSPeer.Application.Features.Scripts.Commands.ConfirmScript
{
	public class ConfirmScriptCommandHandler : IRequestHandler<ConfirmScriptCommand, Unit>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;
		private readonly IMediator _mediator;

		public ConfirmScriptCommandHandler(RsPeerContext db, IRedisService redis, IMediator mediator)
		{
			_db = db;
			_redis = redis;
			_mediator = mediator;
		}

		public async Task<Unit> Handle(ConfirmScriptCommand request, CancellationToken cancellationToken)
		{
			var pendingScript =
				await _db.Scripts.FirstOrDefaultAsync(s =>
					s.Id == request.ScriptId && s.Status == ScriptStatus.Pending, cancellationToken);

			var premiumPercentTaken =
				await _mediator.Send(new GetSiteConfigOrThrowCommand { Key = "premiumScriptPercentTaken" },
					cancellationToken);

			if (pendingScript == null) throw new NotFoundException("PendingScript", request.ScriptId);

			var map = await _db.PendingScripts.FirstOrDefaultAsync(w => w.PendingScriptId == pendingScript.Id,
				cancellationToken);

			var script =
				await _mediator.Send(new GetFullScriptByIdQuery { ScriptId = map.LiveScriptId }, cancellationToken);

			var isFreeToPremium = pendingScript.Type == ScriptType.Premium && script.Type == ScriptType.Free;

			TinyMapper.Map(pendingScript, script);

			script.Status = ScriptStatus.Live;

			if (script.Instances.HasValue && script.Instances.Value == -1)
			{
				script.Instances = null;
			}

			var isNewScript = map.LiveScriptId == map.PendingScriptId;

			if (!isNewScript)
			{
				var oldContent =
					await _db.ScriptContents.FirstOrDefaultAsync(w => w.ScriptId == script.Id, cancellationToken);

				var newContent =
					await _db.ScriptContents.FirstOrDefaultAsync(w => w.ScriptId == pendingScript.Id, cancellationToken);	
				
				if (newContent != null)
				{
					newContent.ScriptId = script.Id;
					_db.ScriptContents.Update(newContent);
					_db.ScriptContents.Remove(oldContent);
				}
			}

			if (!isNewScript)
			{
				_db.Scripts.Remove(pendingScript);		
			}
		
			_db.Scripts.Update(script);
			_db.PendingScripts.Remove(map);

			if (isFreeToPremium)
			{
				await _mediator.Send(new RemoveAllScriptAccessCommand { ScriptId = script.Id }, cancellationToken);
			}
			
			await _db.Data.AddAsync(new Data
			{
				Key = $"script:confirm:{script.Id}:by",
				Value = request.User.Id.ToString()
			}, cancellationToken);

			if (script.Price.HasValue && script.Type == ScriptType.Premium)
			{
				await _mediator.Send(new AddItemCommand
				{
					Upsert = true,
					Description = script.Description,
					Name = script.Name,
					FeesPercent = decimal.Parse(premiumPercentTaken),
					PaymentMethod = PaymentMethod.Tokens,
					Price = script.Price.Value,
					Sku = $"premium-script-{script.Id}"
				}, cancellationToken);
			}

			await _db.SaveChangesAsync(cancellationToken);
			
			var transaction = _redis.GetDatabase().CreateTransaction();
			transaction.KeyDeleteAsync($"{script.Id}_content_bytes", CommandFlags.FireAndForget);
			transaction.StringSetAsync($"script_{script.Id}_details", JsonConvert.SerializeObject(new ScriptDto(script)));
			await transaction.ExecuteAsync(CommandFlags.FireAndForget);
			
			var user = await _mediator.Send(new GetUserByIdQuery { Id = script.UserId }, cancellationToken);
			
			await _mediator.Send(new SendDiscordWebHookCommand
			{
				Type = DiscordWebHookType.ScriptUpdate,
				Message = $"{script.Name} by {user.Username} has been updated to version {script.Version}."
			}, cancellationToken);

			return Unit.Value;
		}
	}
}