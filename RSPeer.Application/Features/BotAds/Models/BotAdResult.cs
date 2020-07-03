using System.Collections.Generic;

namespace RSPeer.Application.Features.BotAds.Models
{
    public class BotAdResult
    {
        public IEnumerable<BotAd> Ads { get; set; }
        public int HideSeconds { get; set; } = 120;
        public bool ShouldShow { get; set; }
    }
}