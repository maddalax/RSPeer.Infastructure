using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Application.Features.Store.Paypal.Commands;
using RSPeer.Application.Features.Store.Paypal.Queries;
using RSPeer.Application.Features.Store.Process.Commands;
using RSPeer.Application.Features.Store.Process.Queries;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers.Admin
{
	[Owner]
	public class AdminOrderController : BaseController
	{
		[HttpPost]
		public async Task<ActionResult<IEnumerable<Order>>> List([FromBody] GetOrdersQuery query)
		{
			return Ok(await Mediator.Send(query));
		}

		[HttpPost]
		public async Task<IActionResult> Refund([FromBody] RefundOrderCommand command)
		{
			command.AdminUserId = (await GetUser()).Id;
			await Mediator.Send(command);
			return Ok();
		}

		[HttpGet]
		public async Task<IActionResult> UnfinishedPaypalOrders()
		{
			return Ok(await Mediator.Send(new GetUnfinishedPaypalOrdersQuery()));
		}

		[HttpPost]
		public async Task<IActionResult> ProcessUnfinishedPaypalOrders()
		{
			return Ok(await Mediator.Send(new PaypalExecuteProcessingOrdersCommand()));
		}
	}
}