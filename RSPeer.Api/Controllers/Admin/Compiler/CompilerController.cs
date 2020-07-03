using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Application.Features.Files.Queries;
using RSPeer.Application.Features.Files.Utility;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers.Admin.Compiler
{
    [Compiler]
    public class CompilerController : BaseController
    {
        public async Task<IActionResult> InuvationJar()
        {
            var name = "inuvation.jar";
            var file = await Mediator.Send(new GetFileQuery { Name = name });
            return File(file, "application/java-archive", name);
        }
        
        public async Task<IActionResult> GetJar([FromQuery] Game game)
        {
            var name = FileHelper.GetFileName(game);
            var file = await Mediator.Send(new GetFileQuery { Name = name });
            return File(file, "application/java-archive", name);
        }
    }
}