using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.Discord.Commands;
using RSPeer.Application.Features.Discord.Models;
using RSPeer.Application.Features.Files.Commands;
using RSPeer.Application.Features.Groups.Queries;
using RSPeer.Domain.Constants;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Bot.Commands.Inuvation
{
    public class GenerateModscriptCommandHandler : IRequestHandler<GenerateModscriptCommand, byte[]>
    {
        private readonly RsPeerContext _db;
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private readonly IMediator _mediator;

        public GenerateModscriptCommandHandler(RsPeerContext db, IHttpClientFactory factory, IConfiguration config, IMediator mediator)
        {
            _db = db;
            _client = factory.CreateClient("JavaWebApi");
            _baseUrl = config.GetValue<string>("JavaWebApi:Endpoint");
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Add("Authorization", config.GetValue<string>("JavaWebApi:Auth"));
            _mediator = mediator;
        }

        public async Task<byte[]> Handle(GenerateModscriptCommand request, CancellationToken cancellationToken)
        {
            var hasAccess = await _mediator.Send(new HasGroupQuery(request.UserId, GroupConstants.InuvationId, GroupConstants.InuvationMaintainerId), cancellationToken);

            if (!hasAccess)
            {
                await _mediator.Send(new SendDiscordWebHookCommand
                {
                    Type = DiscordWebHookType.Log,
                    Message =
                        $"{request.UserId} attempted to download Inuvation modscript, but does not have Inuvation access."
                }, cancellationToken);
                
                throw new AuthorizationException("You do not have access to this.");
            }
            
            var key = !string.IsNullOrEmpty(request.Sha1) ? $"inuvation_modscript_{request.Sha1}" : $"inuvation_modscript_{request.Hash}";
            
            var cached = await _db.Files.AsNoTracking().FirstOrDefaultAsync(w => w.Name == key, cancellationToken);
            
            if (cached != null)
            {
                return cached.File;
            }

            var serialized = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            var result = await _client.PostAsync($"{_baseUrl}inuvation/modscript", new StringContent(serialized, Encoding.Default, "application/json"), cancellationToken);
            var modscript = await result.Content.ReadAsByteArrayAsync();

            if (result.IsSuccessStatusCode && modscript.Length > 0)
            {
                await _mediator.Send(new PutFileCommand
                {
                    Contents = modscript,
                    Name = key,
                    Version = 1
                }, cancellationToken);
            }
            
            return modscript;
        }
    }
}