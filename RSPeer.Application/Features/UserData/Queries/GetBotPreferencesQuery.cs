using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserData.Queries
{
    public class GetBotPreferencesQuery : IRequest<BotPreferences>
    {
        public int UserId { get; set; }
    }
}