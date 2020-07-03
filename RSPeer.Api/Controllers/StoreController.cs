using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.Migration;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Features.Store.Items.Queries;
using RSPeer.Application.Features.Store.Purchase.Commands;
using RSPeer.Application.Features.Store.Purchase.Models;
using RSPeer.Application.Features.Store.Purchase.Queries;
using RSPeer.Common.Enums;
using RSPeer.Domain.Dtos;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers
{
	public class StoreController : BaseController
	{
		[HttpPost]
		[LoggedIn]
		[RateLimit(10, 1)]
		[ReadOnly]
		public async Task<ActionResult<PurchaseItemResult>> Purchase([FromBody] PurchaseItemCommand command)
		{
			command.User = await GetUser();
			return Ok(await Mediator.Send(command));
		}

		[HttpGet]
		[LoggedIn]
		[RateLimit(200, 1)]
		public async Task<ActionResult<ItemDto>> GetItem([FromQuery] string sku)
		{
			return Ok(new ItemDto(await Mediator.Send(new GetItemBySkuQuery { Sku = sku, AllowCached = false })));
		}

		[HttpGet]
		[RateLimit(200, 1)]
		public async Task<ActionResult<IEnumerable<PricePerQuantity>>> PricesPerQuantity([FromQuery] string sku)
		{
			var results = await Mediator.Send(new GetCustomPricesForItemQuery {Sku = sku});
			return Ok(results);
		}
		
		[HttpPost]
		[LoggedIn]
		[RateLimit(25, 1)]
		public async Task<ActionResult<IEnumerable<PricePerQuantity>>> AddPricePerQuantity([FromBody] ModifyPricePerQuantityCommand command)
		{
			command.User = await GetUser();
			command.Action = CrudAction.Create;
			var results = await Mediator.Send(command);
			return Ok(results);
		}
		
			
		[HttpPost]
		[LoggedIn]
		[RateLimit(25, 1)]
		public async Task<IActionResult> RemovePricePerQuantity([FromBody] ModifyPricePerQuantityCommand command)
		{
			command.User = await GetUser();
			command.Action = CrudAction.Delete;
			var results = await Mediator.Send(command);
			return Ok(results);
		}
	}
}