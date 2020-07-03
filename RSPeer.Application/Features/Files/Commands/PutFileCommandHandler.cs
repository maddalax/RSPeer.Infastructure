using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Files.Commands
{
	public class PutFileCommandHandler : IRequestHandler<PutFileCommand, BinaryFile>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;

		public PutFileCommandHandler(RsPeerContext db, IRedisService redis)
		{
			_db = db;
			_redis = redis;
		}

		public async Task<BinaryFile> Handle(PutFileCommand request, CancellationToken cancellationToken)
		{
			var exists = await _db.Files.AsQueryable().Where(w => w.Name == request.Name)
				.Select(w => w.Version).OrderByDescending(w => w).FirstOrDefaultAsync(cancellationToken);
			
			var file = new BinaryFile
			{
				Name = request.Name,
				LastUpdate = DateTimeOffset.UtcNow,
				Version = request.Version == default 
					? exists != default ? exists + (decimal) 0.01 : (decimal) 0.01 
					: request.Version
			};
			
			if (request.Contents == null && request.Stream != null)
			{
				file.File = request.Stream.ToByteArray();
			}
			else
			{
				file.File = request.Contents;
			}

			if (file.File != null && file.File.Length <= 0)
			{
				throw new Exception("Can not put an empty file by name: " + request.Name);
			}
			
			await _db.Files.AddAsync(file, cancellationToken);
			await _db.SaveChangesAsync(cancellationToken);
			var transaction = _redis.GetDatabase().CreateTransaction();
			transaction.StringSetAsync($"file_{request.Name}_latest_version", file.Version.ToString());
			transaction.StringSetAsync($"file_{request.Name}_version_{file.Version}", file.File);
			transaction.StringSetAsync($"file_{request.Name}_latest", file.File);
			await transaction.ExecuteAsync();
			return file;
		}
	}
}