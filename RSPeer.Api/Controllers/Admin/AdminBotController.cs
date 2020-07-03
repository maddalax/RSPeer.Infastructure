using System;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Application.Features.Bot.Commands;
using RSPeer.Application.Features.Bot.Commands.Inuvation;
using RSPeer.Application.Features.Files.Commands;
using RSPeer.Application.Features.Files.Queries;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Contracts;

namespace RSPeer.Api.Controllers.Admin
{
	[Owner]
	public class AdminBotController : BaseController
	{
		private readonly IMediator _mediator;

		public AdminBotController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		[DisableRequestSizeLimit]
		public async Task<IActionResult> UpdateBot(IFormFile file, [FromQuery] bool silent, decimal version)
		{
			if (!file.FileName.Equals("rspeer-client-1.0-jar-with-dependencies-obb.jar"))
			{
				throw new Exception("File name must be rspeer-client-1.0-jar-with-dependencies-obb.jar");
			}
			await _mediator.Send(new UpdateBotCommand
			{
				File = file,
				SilentUpdate = silent,
				Version = version
			});
			return Ok();
		}
		
		[HttpPost]
		[DisableRequestSizeLimit]
		public async Task<IActionResult> UpdateInuvationUpdater(IFormFile file, [FromQuery] decimal version)
		{
			if (!file.FileName.StartsWith("updater"))
			{
				throw new Exception("File name must start with updater.");
			}
			
			await _mediator.Send(new UpdateInuvationUpdaterCommand
			{
				File = file,
				Version = version
			});
			return Ok();
		}
		
		[HttpPost]
		[DisableRequestSizeLimit]
		public async Task<IActionResult> UpdateInuvation(IFormFile file, [FromQuery] bool silent, decimal version)
		{
			if (!file.FileName.Equals("rspeer-inuvation-obfuscated.jar"))
			{
				throw new Exception("File name must be rspeer-inuvation-obfuscated.jar");
			}
			await _mediator.Send(new UpdateInuvationCommand
			{
				File = file,
				SilentUpdate = silent,
				Version = version
			});
			return Ok();
		}
		
		[HttpGet]
		public async Task<IActionResult> ObfuscateBot([FromQuery] string path)
		{
			var bytes = await _mediator.Send(new ObfuscateBotCommand {Path = path});
			return File(bytes, "application/java-archive", "rspeer.jar");
		}
		
		[HttpPost]
		[DisableRequestSizeLimit]
		public async Task<IActionResult> UpdateModScript(IFormFile file)
		{
			await _mediator.Send(new PutFileCommand
			{
				Contents = file.OpenReadStream().ToByteArray(),
				Name = "bot-modscript"
			});
			return Ok();
		}
		
		[HttpPost]
		public async Task<IActionResult> UpdateBotObfuscateConfig(UpdateTextRequest request)
		{
			await _mediator.Send(new PutFileCommand
			{
				Contents = Encoding.Default.GetBytes(request.Text),
				Name = "bot-obfuscate-config.xml"
			});
			return Ok();
		}
		
		[HttpGet]
		public async Task<IActionResult> GetBotObfuscateConfig()
		{
			var file = await _mediator.Send(new GetFileQuery
			{
				Name = "bot-obfuscate-config.xml"
			});
			return Ok(Encoding.Default.GetString(file));
		}
		
		[HttpPost]
		public async Task<IActionResult> UpdateScriptObfuscateConfig(UpdateTextRequest request)
		{
			await _mediator.Send(new PutFileCommand
			{
				Contents = Encoding.Default.GetBytes(request.Text),
				Name = "script-obfuscate-config.xml"
			});
			return Ok();
		}
		
			
		[HttpGet]
		public async Task<IActionResult> GetScriptObfuscateConfig()
		{
			var file = await _mediator.Send(new GetFileQuery
			{
				Name = "script-obfuscate-config.xml"
			});
			return Ok(Encoding.Default.GetString(file));
		}
	}
}