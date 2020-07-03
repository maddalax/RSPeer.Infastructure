using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace RSPeer.Api.Middleware.Authorization
{
    public class CompilerFilter : IAsyncAuthorizationFilter
    {
        private readonly IConfiguration _configuration;

        public CompilerFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var header = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
           
            if (header == null)
            {
                context.Result = new UnauthorizedResult();
                return Task.CompletedTask;
            }

            var key = _configuration.GetValue<string>("Compiler:Token");

            if (!string.IsNullOrEmpty(key) && header == key)
            {
                return Task.CompletedTask;
            }
            
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }
    }
}