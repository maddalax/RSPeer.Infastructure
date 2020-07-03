using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Constants;
using RSPeer.Domain.Dtos;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;
using StackExchange.Redis;

namespace RSPeer.Application.Features.Scripts.Queries.GetScript
{
	public class GetScriptsQueryHandler : IRequestHandler<GetScriptsQuery, IEnumerable<ScriptDto>>
	{
		private readonly RsPeerContext _db;

		public GetScriptsQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<ScriptDto>> Handle(GetScriptsQuery request, CancellationToken cancellationToken)
		{
			List<Script> scripts;

			if (request.Type == ScriptType.Mine && !request.UserId.HasValue)
			{
				return new List<ScriptDto>();
			}

			var queryable = await GetDefaultQueryable(request, cancellationToken);

			if (request.OrderBy == ScriptOrderBy.Users)
			{
				queryable = queryable.OrderByDescending(w => w.TotalUsers);
			}
			else if (request.OrderBy == ScriptOrderBy.Alphabetical)
			{
				queryable = queryable.OrderBy(w => w.Name);
			}
			else if (request.OrderBy == ScriptOrderBy.LastUpdated)
			{
				queryable = queryable.OrderByDescending(w => w.LastUpdate);
			}
			else if (request.OrderBy == ScriptOrderBy.Newest)
			{
				queryable = queryable.OrderBy(w => w.DateAdded);
			}

			if (request.Type != ScriptType.Private && (request.OrderBy == ScriptOrderBy.Featured || request.OrderBy == ScriptOrderBy.FeaturedAllTime))
			{
				var names = await GetMostRunInTimeFrame(request.OrderBy == ScriptOrderBy.Featured
					? TimeSpan.FromHours(24)
					: (TimeSpan?) null);
				queryable = queryable.Include(w => w.User);
				var results = await queryable.Where(w => names.Contains(w.Name))
					.ToListAsync(cancellationToken: cancellationToken);
				var popular = results.OrderBy(w => names.IndexOf(w.Name)).ToList();
				var popularIds = popular.Select(w => w.Id);
				var restOfScripts = await GetDefaultQueryable(request, CancellationToken.None);
				scripts = popular
					.Concat(await restOfScripts.Where(w => !popularIds.Contains(w.Id)).ToListAsync(cancellationToken))
					.ToList();
			}
			else
			{
				scripts = await queryable.ToListAsync(cancellationToken);
			}

			var dtos = scripts.Select(w => new ScriptDto(w));
			return dtos;
		}

		private async Task<IQueryable<Script>> GetDefaultQueryable(GetScriptsQuery request, CancellationToken cancellationToken)
		{
			var queryable = _db.Scripts.AsQueryable();
			queryable = queryable.Include(w => w.User);

			queryable = request.Status.HasValue
				? queryable.Where(w => w.Status == request.Status.Value)
				: queryable.Where(w => w.Status == ScriptStatus.Live);


			var isAdmin = request.UserId.HasValue &&
			              await _db.UserGroups.AnyAsync(w => w.UserId == request.UserId && w.GroupId == GroupConstants.OwnersId, cancellationToken);

			if (!isAdmin)
			{
				queryable = queryable.Where(w => w.Type != ScriptType.HiddenPublic);
			}

			if (request.Category.HasValue)
			{
				queryable = queryable.Where(w => w.Category == request.Category.Value);
			}

			if (request.Game != Game.Both)
			{
				queryable = queryable.Where(w => w.Game == request.Game);
			}

			if (request.Type.HasValue)
			{
				if (request.Type.Value == ScriptType.Mine)
				{
					var now = DateTimeOffset.UtcNow;
					var scriptIds = await _db.ScriptAccess.Where(w => w.UserId == request.UserId)
						.Where(w => !w.Expiration.HasValue || w.Expiration.Value > now).Select(w => w.ScriptId).ToListAsync(cancellationToken);
					queryable = queryable.Where(w => scriptIds.Contains(w.Id));
				}

				else if (request.Type == ScriptType.Private)
				{
					if (!request.UserId.HasValue)
					{
						throw new Exception("You must be logged in to view private scripts.");
					}

					var ourAccess = await _db.PrivateScriptAccess.Where(w => w.UserId == request.UserId)
						.Select(w => w.ScriptId).ToListAsync(cancellationToken: cancellationToken);

					queryable = queryable.Where(w => w.Type == ScriptType.Private && ourAccess.Contains(w.Id));
				}

				else
				{
					queryable = queryable.Where(w => w.Type == request.Type && !w.Disabled);
				}
			}

			else
			{
				if (!isAdmin)
				{
					queryable = queryable.Where(w => w.Type != ScriptType.Private);
				}
			}

			if (!string.IsNullOrEmpty(request.Search))
			{
				queryable = queryable.Where(w => w.Name.ToLower().Contains(request.Search.ToLower())
				                                 || w.Description.ToLower().Contains(request.Search.ToLower()) || w.User.Username
					                                 .ToLower().Contains(request.Search.ToLower())
				);
			}

			return queryable;
		}

		private async Task<List<string>> GetMostRunInTimeFrame(TimeSpan? span)
		{
			var date = span.HasValue ? DateTimeOffset.UtcNow.AddTicks(-span.Value.Ticks) : (DateTimeOffset?) null;
			var query = _db.RunescapeClients
				.Where(w => w.IsRepoScript && w.ScriptName != null);

			if (date.HasValue)
			{
				query = query.Where(w => w.LastUpdate > date);
			}

			return await query.GroupBy(w => w.ScriptName)
				.OrderByDescending(w => w.Count()).Select(w => w.Key)
				.ToListAsync();
		}
	}
}