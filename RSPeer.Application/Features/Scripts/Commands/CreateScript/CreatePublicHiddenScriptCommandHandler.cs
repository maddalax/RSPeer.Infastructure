using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Nelibur.ObjectMapper;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripts.Commands.CreateScript
{
   public class CreatePublicHiddenScriptCommandHandler : IRequestHandler<CreatePublicHiddenScriptCommand, Unit>
	{
		private readonly RsPeerContext _db;

		public CreatePublicHiddenScriptCommandHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<Unit> Handle(CreatePublicHiddenScriptCommand request, CancellationToken cancellationToken)
		{
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
			
			request.Script.Type = ScriptType.HiddenPublic;
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
					content.Content = bytes;
					_db.ScriptContents.Update(content);
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
				
				transaction.Commit();
			}
			
			return Unit.Value;
		}
	}
}