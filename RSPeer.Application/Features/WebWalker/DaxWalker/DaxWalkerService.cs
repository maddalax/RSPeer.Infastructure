using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.WebWalker.Base;

namespace RSPeer.Application.Features.WebWalker.DaxWalker
{
    public class DaxWalkerService : IWebWalker
    {
        private readonly HttpClient _client;
        private readonly Dictionary<string, string> _keys = new Dictionary<string, string>();

        public DaxWalkerService(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient("DaxWebWalker");
            _client.BaseAddress = new Uri(config.GetValue<string>("WebWalker:DaxEndpoint"));
        }

        public async Task<object> GenerateBankPath(JObject payload, string keys)
        {
            return await MakeRequest(payload, "/walker/generateBankPaths", keys);
        }

        public async Task<object> GeneratePath(JObject payload, string keys)
        {
            return await MakeRequest(payload, "/walker/generatePaths", keys);
        }

        private async Task<object> MakeRequest(JObject payload, string path, string keys)
        {
            var all = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(keys))
            {
                var split = keys.Split(",");
                foreach (var pair in split.Select(s => s.Split(":"))
                    .Where(pair => pair.Length == 2).ToDictionary(pair => pair[0], pair => pair[1]))
                {
                    all.Add(pair.Key, pair.Value);
                }
            }

            foreach (var (key, value) in _keys)
            {
                all.Add(key, value);
            }
            string content;
            var code = HttpStatusCode.InternalServerError;
            foreach (var pair in all)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("key", pair.Key);
                request.Headers.Add("secret", pair.Value);
                request.Content = new StringContent(payload.ToString(), Encoding.Default, "application/json");
                var result = await _client.SendAsync(request, CancellationToken.None);
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    code = result.StatusCode;
                    continue;
                }
                content = await result.Content.ReadAsStringAsync();
                return JArray.Parse(content);
            }

            throw new WebPathException("Failed to generate web path. Response Code: " + code);
        }
    }
}