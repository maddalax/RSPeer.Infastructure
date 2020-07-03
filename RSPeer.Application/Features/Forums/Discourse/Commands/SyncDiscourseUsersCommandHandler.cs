using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RSPeer.Application.Features.UserManagement.Users.Models;
using RSPeer.Common.File;
using RSPeer.Common.Linq;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Forums.Discourse.Commands
{
    public class SyncDiscourseUsersCommandHandler : IRequestHandler<SyncDiscourseUsersCommand, User>
    {
        private readonly string _discourseApiUsername;
        private readonly string _discourseApiToken;
        private readonly string _discourseSsoToken;
        private readonly string _discourseUrl;
        private readonly HttpClient _client;
        private readonly RsPeerContext _db;

        public SyncDiscourseUsersCommandHandler(IConfiguration configuration, IHttpClientFactory factory,
            RsPeerContext db)
        {
            _discourseApiUsername = configuration.GetValue<string>("Forums:DiscourseApiUsername");
            _discourseApiToken = configuration.GetValue<string>("Forums:DiscourseApiKey");
            _discourseUrl = configuration.GetValue<string>("Forums:DiscourseUrl");
            _discourseSsoToken = configuration.GetValue<string>("Forums:Token");
            _client = factory.CreateClient("Discourse");
            _client.DefaultRequestHeaders.Remove("Api-Key");
            _client.DefaultRequestHeaders.Remove("Api-Username");
            _client.DefaultRequestHeaders.Add("Api-Key", _discourseApiToken);
            _client.DefaultRequestHeaders.Add("Api-Username", _discourseApiUsername);
            _db = db;
        }

        public async Task<User> Handle(SyncDiscourseUsersCommand request, CancellationToken cancellationToken)
        {
            var queryable = _db.Users.AsNoTracking();
            if (!string.IsNullOrEmpty(request.Email))
            {
                queryable = queryable.Where(w => w.Email == request.
                                                 Email);
                var user = await queryable.FirstOrDefaultAsync(cancellationToken);
                return await Sync(user, cancellationToken);
            }
            
            await queryable.ForEachBulkAsync(100, async list =>
            {
                await list.ForEachAsync(6, async user =>
                {
                    await Sync(user, cancellationToken);
                });
            });
            return null;
        }

        private async Task<User> Sync(User user, CancellationToken token)
        {
            if (user == null)
            {
                return null;
            }

            var s = BuildResult(user);
            var qs = $"sso={s.Payload}&sig={s.Hash}";
            var result = await _client.PostAsync($"{_discourseUrl}admin/users/sync_sso.json?{qs}",
                new StringContent(string.Empty), token);
            
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to sync " + user.Id + " " + await result.Content.ReadAsStringAsync());
            }

            return user;
        }

        private DiscourseSignInResult BuildResult(User user)
        {
            var discourse = new DiscourseSignInRequest {Nonce = "123", Redirect = _discourseUrl, User = user};
            var discourseSignInResult = new DiscourseSignInResult(discourse);
            DiscourseSignInResult.CalculateHash(discourseSignInResult, _discourseSsoToken);
            return discourseSignInResult;
        }
    }
}