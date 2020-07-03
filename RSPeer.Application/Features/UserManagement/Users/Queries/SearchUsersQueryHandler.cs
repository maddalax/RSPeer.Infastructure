using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
	public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<User>>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public SearchUsersQueryHandler(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<IEnumerable<User>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
		{
			var term = request.SearchTerm?.ToLower()?.Trim();
			if (string.IsNullOrEmpty(term))
			{
				return new List<User>();
			}

			var results = new List<User>();
			
			if (int.TryParse(term, out var id))
			{
				var result =
					await _mediator.Send(new GetUserByIdQuery { Id = id, IncludeGroups = true, AllowCached = false },
						cancellationToken);

				if (result != null)
				{
					results.Add(result);
				}
			}
			
			results.AddRange(await _db.Users
				.Where(w =>
					w.Email.ToLower().Contains(term) 
					|| w.Username.ToLower().Contains(term)
				)
				.Take(50)
				.ToListAsync(cancellationToken));

			var byDiscord = await _db.Users
				.Include(w => w.DiscordAccount)
				.Where(w => w.DiscordAccount != null && (w.DiscordAccount.DiscordUsername.ToLower().Contains(term)
				                                         || w.DiscordAccount.DiscordUserId.ToLower().Contains(term) ||
				                                         w.DiscordAccount.Discriminator.ToString().ToLower()
					                                         .Contains(term)))
				.ToListAsync(cancellationToken);
			
			results.AddRange(byDiscord);

			results.AddRange(await _db.Orders.Where(w => w.PaypalId.ToLower() == term || w.PaypalTransactionId.ToLower() == term)
				.Select(w => w.User)
				.ToListAsync(cancellationToken));

			return results
				.OrderByDescending(w => w.DiscordAccount?.DateVerified ?? DateTimeOffset.MinValue)
				.Distinct();
		}
	}
}