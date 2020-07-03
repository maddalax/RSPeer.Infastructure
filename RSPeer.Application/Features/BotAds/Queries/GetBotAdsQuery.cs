using System;
using MediatR;
using RSPeer.Application.Features.BotAds.Models;

namespace RSPeer.Application.Features.BotAds.Queries
{
    public class GetBotAdsQuery : IRequest<BotAdResult>
    {
        public Guid ClientTag { get; set; }
        public int UserId { get; set; }
    }
}