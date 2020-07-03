using System;
using Microsoft.AspNetCore.Mvc;

namespace RSPeer.Api.Controllers
{
    [Route("/")]
    public class IndexController : Controller
    {
        public IActionResult Index()
        {

            return Ok(new {Message = "RSPeer Api Version " + Program.Version});
        }
    }
}