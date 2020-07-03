using System;

namespace RSPeer.Application.Features.BotAds.Models
{
    public class BotAd
    {
        public string Image { get; set; }
        public string Redirect { get; set; }
        public DateTimeOffset ExpirationDate { get; set; }
        public bool Expired => ExpirationDate != DateTimeOffset.MinValue && ExpirationDate < DateTimeOffset.Now;
        public string Meta { get; set; }
        public string Username { get; set; }
        public bool Enabled { get; set; }
    }
}