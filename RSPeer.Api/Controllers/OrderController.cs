using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Features.Store.Process.Queries;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers
{
	[LoggedIn]
	public class OrderController : BaseController
	{
		[RateLimit(25, 1)]
		[HttpPost]
		public async Task<ActionResult<IEnumerable<Order>>> List([FromBody] GetOrdersQuery query)
		{
			var user = await GetUser();
			query.UserId = user.Id;
			query.IncludeItem = true; 
			return Ok(await Mediator.Send(query));
		}
		
		[RateLimit(25, 1)]
		[HttpGet]
		public async Task<IActionResult> Expiration([FromQuery] string sku)
		{
			var user = await GetUser();
			var expiration = await Mediator.Send(new GetOrdersExpirationQuery {Sku = sku, UserId = user.Id});
			return Ok(new {date = expiration});
		}
		
		[RateLimit(25, 1)]
		[HttpPost]
		public async Task<IActionResult> Expiration([FromBody] IEnumerable<Order> orders)
		{
			var user = await GetUser();
			var expiration = await Mediator.Send(new GetOrdersExpirationQuery {Orders = orders, UserId = user.Id});
			return Ok(new {date = expiration});
		}
		
		[RateLimit(25, 1)]
		public async Task<ActionResult<IEnumerable<Order>>> List()
		{
			var user = await GetUser();
			return Ok(user == null
				? new List<Order>()
				: await Mediator.Send(new GetOrdersQuery
				{
					IncludeItem = true,
					UserId = user.Id
				}));
		}
		
		[RateLimit(25, 1)]
		public async Task<ActionResult<Order>> Get([FromQuery] int id)
		{
			var user = await GetUser();
			return Ok(user == null ? null : await Mediator.Send(new GetOrderByIdQuery
			{
				IncludeItem = true,
				UserId = user.Id,
				OrderId = id,
				IsAdmin = user.IsOwner
			}));
		}
	}
}