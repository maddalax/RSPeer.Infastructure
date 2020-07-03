using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Api.Utilities;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.Groups.Queries;
using RSPeer.Application.Features.Instance.Commands;
using RSPeer.Application.Features.Instance.Queries;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Constants;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers
{
	[LoggedIn]
	public class ClientController : BaseController
	{
		private readonly IConfiguration _configuration;
		private readonly IRedisService _redis;
		private readonly IMediator _mediator;

		public ClientController(IConfiguration configuration, IRedisService redis, IMediator mediator)
		{
			_configuration = configuration;
			_redis = redis;
			_mediator = mediator;
		}

		[HttpPost]
		[RateLimit(2000, 1)]
		public async Task<IActionResult> Ping()
		{
			var userId = await GetUserId();
			var body = await Request.Body.ToStringAsync();
			var id = GetConnectionId();
			
			var client = await Mediator.Send(new ParseClientInfoCommand
			{
				Identifier = id,
				RawClientInfo = body,
				UserId = userId
			});

			if (client == null)
			{
				return BadRequest("Client data returned null after parsing.");
			}

			client.Game = Game.Osrs;
			
			try
			{
				await _mediator.Send(new SaveClientInfoBulkCommand
				{
					Clients = new[] {client}
				});
			}
			catch (Exception)
			{
				// ignored
			}
			
			return Ok(DateTimeOffset.Now.ToUnixTimeMilliseconds());
		}
		
		[HttpPost]
		[RateLimit(2000, 1)]
		public async Task<IActionResult> Inuvation()
		{
			var userId = await GetUserId();
			var body = await Request.Body.ToStringAsync();
			var id = GetConnectionId();

			var command = new ParseClientInfoCommand
			{
				Identifier = id,
				RawClientInfo = body,
				UserId = userId,
				Game = Game.Rs3,
				IsXor = true
			};
			
			var client = await Mediator.Send(command);
			
			if (client == null)
			{
				return BadRequest("Client data returned null after parsing.");
			}
			
			client.Game = Game.Rs3;

			try
			{
				await _mediator.Send(new SaveClientInfoBulkCommand
				{
					Clients = new[] {client}
				});
			}
			catch (Exception)
			{
				// ignored
			}

			var hasAccess = await Mediator.Send(new HasGroupQuery(userId, GroupConstants.InuvationId,
				GroupConstants.InuvationMaintainerId));

			if (!hasAccess)
			{
				return Unauthorized("You do not have access to Inuvation.");
			}
			
			var xor = StringExtensions.Xor(client.Hash, _configuration.GetValue<string>("Encryption:XorInuvation"));
			var base64 = Convert.ToBase64String(xor);
			return Ok(base64);
		}
		
		[HttpPost]
		[RateLimit(2000, 1)]
		public async Task<IActionResult> Update()
		{
			var userId = await GetUserId();
			var body = await Request.Body.ToStringAsync();
			var id = GetConnectionId();

			var command = new ParseClientInfoCommand
			{
				Identifier = id,
				RawClientInfo = body,
				UserId = userId,
				IsXor = true
			};
			
			var client = await Mediator.Send(command);
			
			if (client == null)
			{
				return BadRequest("Client data returned null after parsing.");
			}
			
			client.Game = Game.Osrs;

			try
			{
				await _mediator.Send(new SaveClientInfoBulkCommand
				{
					Clients = new[] {client}
				});
			}
			catch (Exception)
			{
				// ignored
			}

			var over = await Mediator.Send(new IsClientOverLimitQuery {Tag = id, UserId = userId});

			if (over)
			{
				return BadRequest("You are currently over your instance limit.");
			}
			
			var xor = StringExtensions.Xor(client.Hash, _configuration.GetValue<string>("Encryption:Xor"));
			var base64 = Convert.ToBase64String(xor);
			return Ok(base64);
		}

		[HttpPost]
		[RateLimit(2000, 1)]
		public async Task<IActionResult> Close()
		{
			var id = GetConnectionId();
			var userId = await GetUserId();
			var transaction = _redis.GetDatabase().CreateTransaction();
			transaction.SetRemoveAsync($"{userId}_running_client", id.ToString());
			transaction.KeyDeleteAsync($"{id.ToString()}_client_details");
			await transaction.ExecuteAsync(StackExchange.Redis.CommandFlags.FireAndForget);
			return Ok();
		}

		private new async Task<int> GetUserId()
		{
			if (HttpContext.Items.ContainsKey("UserId"))
			{
				return int.Parse(HttpContext.Items["UserId"].ToString());
			}

			var context = HttpContext;
			var session = HttpUtilities.TryGetSession(context);

			if (string.IsNullOrEmpty(session))
			{
				return -1;
			}

			var id = await Mediator.Send(new GetUserIdBySessionQuery { Session = session });
			
			if (id.HasValue)
			{
				HttpContext.Items["UserId"] = id;
			}

			return id ?? -1;
		}

		private Guid GetConnectionId()
		{
			var id = HttpContext.Request.Query["clientId"].FirstOrDefault();
			if (Guid.TryParse(id, out var parsed))
			{
				return parsed;
			}
			throw new NotFoundException("Identifier", id ?? "clientId");
		}
	}
}