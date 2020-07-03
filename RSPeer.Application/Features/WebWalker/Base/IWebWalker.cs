using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RSPeer.Application.Features.WebWalker.Base
{
    public interface IWebWalker
    {
        Task<object> GeneratePath(JObject payload, string? keys);
        Task<object> GenerateBankPath(JObject payload, string? keys);
    }
}