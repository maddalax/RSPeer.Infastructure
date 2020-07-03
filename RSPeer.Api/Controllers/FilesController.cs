using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.Migration;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Features.Files.Commands;
using RSPeer.Application.Features.Files.Queries;
using RSPeer.Infrastructure.File.Base;

namespace RSPeer.Api.Controllers
{
	public class FilesController : BaseController
	{
		private readonly IFileStorage _storage;

		public FilesController(IFileStorage storage)
		{
			_storage = storage;
		}

		[HttpPost]
		[Owner]
		[ReadOnly]
		public async Task<IActionResult> PutString(PutFileCommand command)
		{
			await Mediator.Send(command);
			return Ok();
		}

		[HttpPost]
		[Owner]
		[RateLimit(5, 1)]
		[ReadOnly]
		public async Task<IActionResult> PutFile(IFormFile file)
		{
			using (var stream = file.OpenReadStream())
			{
				await _storage.PutStream(stream, $"uploads/user/{Guid.NewGuid()}.png");
			}
			return Ok();
		}

		[HttpGet]
		[RateLimit(200, 1)]
		public async Task<IActionResult> GetString([FromQuery] string name)
		{
			var result = await Mediator.Send(new GetFileQuery { Name = name });
			return Ok(result == null ? null : Encoding.Default.GetString(result));
		}
	}
}