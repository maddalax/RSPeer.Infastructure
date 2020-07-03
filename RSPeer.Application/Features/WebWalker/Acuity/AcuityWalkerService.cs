using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.WebWalker.Base;

namespace RSPeer.Application.Features.WebWalker.Acuity
{
    public class AcuityWalkerService : IWebWalker
    {
        private readonly HttpClient _client;

        public AcuityWalkerService(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient("AcuityWebWalker");
            _client.BaseAddress = new Uri(config.GetValue<string>("WebWalker:AcuityEndpoint"));
        }

        public async Task<object> GeneratePath(JObject payload, string keys)
        {
            return await MakeRequest(payload, "/web/path/find");
        }

        public async Task<object> GenerateBankPath(JObject payload, string keys)
        {
            return await GeneratePath(payload, keys);
        }

        private async Task<object> MakeRequest(JObject payload, string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new StringContent(payload.ToString(), Encoding.Default, "application/json")
            };
            var result = await _client.SendAsync(request, CancellationToken.None);
            var content = await result.Content.ReadAsStringAsync();
            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw new WebPathException("Failed to generate web path. " + content);
            }

            return JObject.Parse(content);
        }
    }
}