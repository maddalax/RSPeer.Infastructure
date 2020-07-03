using System;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RSPeer.Common.Extensions;

namespace RSPeer.Application.Features.UserManagement.Users.Models
{
    public class DiscourseSignInResult : UserSignInResult
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("name")] public string Name => Username;

        [JsonProperty("bio")] public string Bio { get; set; }

        [JsonProperty("admin")]
        public bool Admin { get; set; }

        [JsonProperty("moderator")]
        public bool Moderator { get; set; }

        [JsonProperty("suppress_welcome_message")]
        public bool SuppressWelcomeMessage { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        public string Redirect { get; set; }

        public string Hash { get; set; }

        public string Payload { get; set; }

        [JsonProperty("add_groups")]
        public string Groups { get; set; }

        public DiscourseSignInResult(DiscourseSignInRequest request)
        {
            var user = request.User;

            Email = user.Email;
            ExternalId = user.Id.ToString();
            Username = user.Username;
            Admin = user.IsOwner;
            Moderator = user.IsModerator;
            SuppressWelcomeMessage = true;
            Nonce = request.Nonce;
            Redirect = request.Redirect;
            Groups = "Test Group";
        }

        public static void CalculateHash(DiscourseSignInResult result, string secret)
        {
            var json = JsonConvert.SerializeObject(result);
            var obj = JObject.Parse(json);
            var query = String.Join("&",
                obj.Children().Cast<JProperty>()
                    .Where(w => !string.IsNullOrEmpty(w.Value.ToString()))
                    .Where(w => w.Name != "Redirect")
                    .Select(jp =>
                    {
                        var val = jp.Value.ToString();
                        if (val == "True" || val == "False")
                        {
                            val = val.ToLower();
                        }
                        return jp.Name + "=" + HttpUtility.UrlEncode(val);
                    }));
            result.Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(query));
            result.Hash = HashingExtensions.SHA256(result.Payload, secret);
        }
    }
}