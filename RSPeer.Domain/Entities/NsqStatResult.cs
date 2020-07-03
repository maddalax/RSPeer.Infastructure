using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RSPeer.Domain.Entities
{
    public class NsqStatResult
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("health")]
        public string Health { get; set; }

        [JsonPropertyName("start_time")]
        public long StartTime { get; set; }

        [JsonPropertyName("topics")]
        public List<Topic> Topics { get; set; }
    }
    
    public class Topic
    {
        [JsonPropertyName("topic_name")]
        public string TopicName { get; set; }

        [JsonPropertyName("channels")]
        public List<Channel> Channels { get; set; }
    }

    public class Channel
    {
        [JsonPropertyName("channel_name")]
        public string ChannelName { get; set; }
        
        [JsonPropertyName("clients")]
        public List<Client> Clients { get; set; }
    }

    public class Client
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }
    }

}