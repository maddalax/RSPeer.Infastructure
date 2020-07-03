using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Nelibur.ObjectMapper;
using Newtonsoft.Json;
using RSPeer.Application.Features.ScriptAccesses.Commands;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Dtos;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;
using CommandFlags = StackExchange.Redis.CommandFlags;

namespace RSPeer.Application.Features.Scripts.Commands.CreateScript
{
	public class CreatePrivateScriptCommandHandler : IRequestHandler<CreatePrivateScriptCommand, Unit>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;
		private readonly IMediator _mediator;

		public CreatePrivateScriptCommandHandler(RsPeerContext db, IRedisService redis, IMediator mediator)
		{
			_db = db;
			_redis = redis;
			_mediator = mediator;
		}

		public async Task<Unit> Handle(CreatePrivateScriptCommand request, CancellationToken cancellationToken)
		{
			if (!request.Script.Name.StartsWith($"{request.User.Username}'s"))
			{
				request.Script.Name = $"{request.User.Username}'s {request.Script.Name}".Trim();
			}

			var exists = await _db.Scripts.FirstOrDefaultAsync(w => w.Name.ToLower() == request.Script.Name.ToLower() || w.Id == request.Script.Id, 
				cancellationToken);

			if (exists != null && request.Script.Id != exists.Id)
			{
				throw new Exception("A script by that name already exists.");
			}
			
			if (exists != null && exists.UserId != request.User.Id)
			{
				throw new Exception("A script by that name already exists.");
			}

			if (exists != null && exists.Type != ScriptType.Private)
			{
				throw new Exception("Existing non private scripts may not be converted to private.");
			}

			if (request.File.Length > 5e+7)
			{
				throw new Exception("Max file size is 50mb.");
			}

			request.Script.Type = ScriptType.Private;
			request.Script.Price = null;
			request.Script.Instances = null;
			request.Script.Status = ScriptStatus.Live;
			request.Script.ForumThread = "https://rspeer.org";
			request.Script.UserId = request.User.Id;

			var bytes = request.File.OpenReadStream().ToByteArray();

			using (var transaction = await _db.Database.BeginTransactionAsync(cancellationToken))
			{
				if (exists != null)
				{
					exists = TinyMapper.Map(request.Script, exists);
					
					var content =
						await _db.ScriptContents.FirstOrDefaultAsync(w => w.ScriptId == exists.Id, cancellationToken);

					if (content == null)
					{
						await _db.ScriptContents.AddAsync(new ScriptContent
						{
							Content = bytes,
							ScriptId = exists.Id
						}, cancellationToken);
					}
					else
					{
						content.Content = bytes;
						_db.ScriptContents.Update(content);
					}
					
					_db.Scripts.Update(exists);
					await _db.SaveChangesAsync(cancellationToken);
				}
				else
				{
					request.Script.Id = 0;
					await _db.Scripts.AddAsync(request.Script, cancellationToken);
					await _db.SaveChangesAsync(cancellationToken);
					await _db.ScriptContents.AddAsync(new ScriptContent
					{
						ScriptId = request.Script.Id,
						Content = bytes
					}, cancellationToken);
					await _db.SaveChangesAsync(cancellationToken);
				}

				await _mediator.Send(new AddPrivateScriptAccessCommand
					{ ScriptId = request.Script.Id, UserId = request.User.Id, RequestingUserId = request.User.Id }, cancellationToken);
				
				transaction.Commit();

				var trans = _redis.GetDatabase().CreateTransaction();
				trans.KeyDeleteAsync($"{request.Script.Id}_content_bytes", StackExchange.Redis.CommandFlags.FireAndForget);
				trans.StringSetAsync($"script_{request.Script.Id}_details", JsonConvert.SerializeObject(new ScriptDto(request.Script)));
				await trans.ExecuteAsync(CommandFlags.FireAndForget);
			}
			
			return Unit.Value;
		}
	}
}