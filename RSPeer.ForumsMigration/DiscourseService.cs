using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.ForumsMigration
{
    public class DiscourseService
    {
        private readonly string _discourseApiUsername;
        private readonly string _discourseApiToken;
        private readonly string _discourseUrl;
        private readonly HttpClient _client;
        private readonly DiscourseContext _db;
        private static readonly ConcurrentDictionary<string, string> usernameMap = new ConcurrentDictionary<string, string>();

        public DiscourseService(IConfiguration configuration, IHttpClientFactory factory, DiscourseContext db)
        {
            _discourseApiUsername = configuration.GetValue<string>("Forums:DiscourseApiUsername");
            _discourseApiToken = configuration.GetValue<string>("Forums:DiscourseApiKey");
            _discourseUrl = configuration.GetValue<string>("Forums:DiscourseUrl");
            _client = factory.CreateClient("Discourse");
            _client.DefaultRequestHeaders.Remove("Api-Key");
            _client.DefaultRequestHeaders.Add("Api-Key", _discourseApiToken);
            _db = db;
        }


        private StringContent MakeContent(object value)
        {
            return new StringContent(JsonConvert.SerializeObject(value, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), Encoding.Default,
                "application/json");
        }

        public async Task<int> CreateTopic(DiscourseTopicRequest topicRequest, User user)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, $"{_discourseUrl}posts.json");
            message.Headers.Add("Api-Username", await GetDiscourseUsername(user.Username));
            message.Content = MakeContent(topicRequest);
            var result = await _client.SendAsync(message);
            var content = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Failed to create topic. " + content);
            }

            return JObject.Parse(content).Value<int>("topic_id");
        }

        public async Task<int> CreatePost(DiscoursePostRequest request, User user)
        {
            if (user == null)
            {
                throw new Exception("Unable to create post for " + request.TopicId + ". User is null.");
            }

            var message = new HttpRequestMessage(HttpMethod.Post, $"{_discourseUrl}posts.json");
            message.Headers.Add("Api-Username", await GetDiscourseUsername(user.Username));
            message.Content = MakeContent(request);
            var result = await _client.SendAsync(message);
            var content = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Failed to create post. " + content);
            }
            return JObject.Parse(content).Value<int>("id");
        }

        public int? GetCategory(int category)
        {
            if (CategoryMap.ContainsKey(category))
            {
                return CategoryMap[category];
            }

            return null;
        }

        private async Task<string> GetDiscourseUsername(string username)
        {
            if (usernameMap.ContainsKey(username))
            {
                usernameMap.TryGetValue(username, out var cached);
                return cached;
            }
            var real = await _db.Users.Where(w => w.Name == username).Select(w => w.Username).FirstOrDefaultAsync();
            if (real != null)
            {
                usernameMap.TryAdd(username, real);
            }
            return real ?? username;
        }

        private static readonly Dictionary<int, int> CategoryMap = new Dictionary<int, int>
        {
            {16, 5},
            {12, 2},
            {11, 2},
            {23, 14},
            {24, 14},
            {26, 14},
            {6, 13},
            {8, 15},
            {27, 16},
            {9, 19},
            {22, 18},
            {13, 6},
            {14, 7},
            {15, 9},
            {17, 10},
            {18, 11},
            {2, 20},
            {19, 21},
            {20, 22},
            {10, 2}
        };
    }

    public class DiscourseUpdatePostRequest
    {
        [JsonProperty("post[raw]")]
        public string Raw { get; set; }

        [JsonProperty("post[raw_old]")]
        public string RawOld { get; set; }

        [JsonProperty("post[edit_reason]")]
        public string EditReason { get; set; }

        [JsonProperty("post[cooked]")]
        public string Cooked { get; set; }
    }

    public class DiscourseTopicRequest
    {
        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("raw")] public string Content { get; set; }

        [JsonProperty("category")] public int? Category { get; set; }

        [JsonProperty("created_at")] public string CreatedAt { get; set; }
    }

    public class DiscoursePostRequest
    {
        [JsonProperty("topic_id")] public int TopicId { get; set; }

        [JsonProperty("raw")] public string Content { get; set; }

        [JsonProperty("created_at")] public string CreatedAt { get; set; }
    }

    public partial class DiscourseFullTopic
    {
        [JsonProperty("post_stream")] public PostStream PostStream { get; set; }

        [JsonProperty("timeline_lookup")] public long[][] TimelineLookup { get; set; }

        [JsonProperty("suggested_topics")] public SuggestedTopic[] SuggestedTopics { get; set; }

        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("fancy_title")] public string FancyTitle { get; set; }

        [JsonProperty("posts_count")] public long PostsCount { get; set; }

        [JsonProperty("created_at")] public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("views")] public long Views { get; set; }

        [JsonProperty("reply_count")] public long ReplyCount { get; set; }

        [JsonProperty("like_count")] public long LikeCount { get; set; }

        [JsonProperty("last_posted_at")] public DateTimeOffset LastPostedAt { get; set; }

        [JsonProperty("visible")] public bool Visible { get; set; }

        [JsonProperty("closed")] public bool Closed { get; set; }

        [JsonProperty("archived")] public bool Archived { get; set; }

        [JsonProperty("has_summary")] public bool HasSummary { get; set; }

        [JsonProperty("archetype")] public string Archetype { get; set; }

        [JsonProperty("slug")] public string Slug { get; set; }

        [JsonProperty("category_id")] public long CategoryId { get; set; }

        [JsonProperty("word_count")] public long WordCount { get; set; }

        [JsonProperty("deleted_at")] public object DeletedAt { get; set; }

        [JsonProperty("user_id")] public long UserId { get; set; }

        [JsonProperty("featured_link")] public object FeaturedLink { get; set; }

        [JsonProperty("pinned_globally")] public bool PinnedGlobally { get; set; }

        [JsonProperty("pinned_at")] public object PinnedAt { get; set; }

        [JsonProperty("pinned_until")] public object PinnedUntil { get; set; }

        [JsonProperty("draft")] public object Draft { get; set; }

        [JsonProperty("draft_key")] public string DraftKey { get; set; }

        [JsonProperty("draft_sequence")] public long DraftSequence { get; set; }

        [JsonProperty("unpinned")] public object Unpinned { get; set; }

        [JsonProperty("pinned")] public bool Pinned { get; set; }

        [JsonProperty("current_post_number")] public long CurrentPostNumber { get; set; }

        [JsonProperty("highest_post_number")] public long HighestPostNumber { get; set; }

        [JsonProperty("deleted_by")] public object DeletedBy { get; set; }

        [JsonProperty("has_deleted")] public bool HasDeleted { get; set; }

        [JsonProperty("actions_summary")] public WelcomeActionsSummary[] ActionsSummary { get; set; }

        [JsonProperty("chunk_size")] public long ChunkSize { get; set; }

        [JsonProperty("bookmarked")] public object Bookmarked { get; set; }

        [JsonProperty("topic_timer")] public object TopicTimer { get; set; }

        [JsonProperty("private_topic_timer")] public object PrivateTopicTimer { get; set; }

        [JsonProperty("message_bus_last_id")] public long MessageBusLastId { get; set; }

        [JsonProperty("participant_count")] public long ParticipantCount { get; set; }

        [JsonProperty("details")] public Details Details { get; set; }
    }

    public partial class WelcomeActionsSummary
    {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("count")] public long Count { get; set; }

        [JsonProperty("hidden")] public bool Hidden { get; set; }

        [JsonProperty("can_act")] public bool CanAct { get; set; }
    }

    public partial class Details
    {
        [JsonProperty("notification_level")] public long NotificationLevel { get; set; }

        [JsonProperty("can_move_posts")] public bool CanMovePosts { get; set; }

        [JsonProperty("can_edit")] public bool CanEdit { get; set; }

        [JsonProperty("can_delete")] public bool CanDelete { get; set; }

        [JsonProperty("can_remove_allowed_users")]
        public bool CanRemoveAllowedUsers { get; set; }

        [JsonProperty("can_invite_to")] public bool CanInviteTo { get; set; }

        [JsonProperty("can_create_post")] public bool CanCreatePost { get; set; }

        [JsonProperty("can_reply_as_new_topic")]
        public bool CanReplyAsNewTopic { get; set; }

        [JsonProperty("can_flag_topic")] public bool CanFlagTopic { get; set; }

        [JsonProperty("can_convert_topic")] public bool CanConvertTopic { get; set; }

        [JsonProperty("can_review_topic")] public bool CanReviewTopic { get; set; }

        [JsonProperty("can_remove_self_id")] public long CanRemoveSelfId { get; set; }

        [JsonProperty("participants")] public Participant[] Participants { get; set; }

        [JsonProperty("created_by")] public CreatedBy CreatedBy { get; set; }

        [JsonProperty("last_poster")] public CreatedBy LastPoster { get; set; }
    }

    public partial class CreatedBy
    {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("avatar_template")] public string AvatarTemplate { get; set; }
    }

    public partial class Participant
    {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("avatar_template")] public string AvatarTemplate { get; set; }

        [JsonProperty("post_count")] public long PostCount { get; set; }

        [JsonProperty("primary_group_name")] public object PrimaryGroupName { get; set; }

        [JsonProperty("primary_group_flair_url")]
        public object PrimaryGroupFlairUrl { get; set; }

        [JsonProperty("primary_group_flair_color")]
        public object PrimaryGroupFlairColor { get; set; }

        [JsonProperty("primary_group_flair_bg_color")]
        public object PrimaryGroupFlairBgColor { get; set; }
    }

    public partial class PostStream
    {
        [JsonProperty("posts")] public Post[] Posts { get; set; }

        [JsonProperty("stream")] public long[] Stream { get; set; }
    }

    public partial class Post
    {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("avatar_template")] public string AvatarTemplate { get; set; }

        [JsonProperty("created_at")] public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("cooked")] public string Cooked { get; set; }

        [JsonProperty("post_number")] public long PostNumber { get; set; }

        [JsonProperty("post_type")] public long PostType { get; set; }

        [JsonProperty("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("reply_count")] public long ReplyCount { get; set; }

        [JsonProperty("reply_to_post_number")] public object ReplyToPostNumber { get; set; }

        [JsonProperty("quote_count")] public long QuoteCount { get; set; }

        [JsonProperty("incoming_link_count")] public long IncomingLinkCount { get; set; }

        [JsonProperty("reads")] public long Reads { get; set; }

        [JsonProperty("score")] public double Score { get; set; }

        [JsonProperty("yours")] public bool Yours { get; set; }

        [JsonProperty("topic_id")] public long TopicId { get; set; }

        [JsonProperty("topic_slug")] public string TopicSlug { get; set; }

        [JsonProperty("display_username")] public string DisplayUsername { get; set; }

        [JsonProperty("primary_group_name")] public object PrimaryGroupName { get; set; }

        [JsonProperty("primary_group_flair_url")]
        public object PrimaryGroupFlairUrl { get; set; }

        [JsonProperty("primary_group_flair_bg_color")]
        public object PrimaryGroupFlairBgColor { get; set; }

        [JsonProperty("primary_group_flair_color")]
        public object PrimaryGroupFlairColor { get; set; }

        [JsonProperty("version")] public long Version { get; set; }

        [JsonProperty("can_edit")] public bool CanEdit { get; set; }

        [JsonProperty("can_delete")] public bool CanDelete { get; set; }

        [JsonProperty("can_recover")] public object CanRecover { get; set; }

        [JsonProperty("can_wiki")] public bool CanWiki { get; set; }

        [JsonProperty("read")] public bool Read { get; set; }

        [JsonProperty("user_title")] public object UserTitle { get; set; }

        [JsonProperty("actions_summary")] public PostActionsSummary[] ActionsSummary { get; set; }

        [JsonProperty("moderator")] public bool Moderator { get; set; }

        [JsonProperty("admin")] public bool Admin { get; set; }

        [JsonProperty("staff")] public bool Staff { get; set; }

        [JsonProperty("user_id")] public long UserId { get; set; }

        [JsonProperty("hidden")] public bool Hidden { get; set; }

        [JsonProperty("trust_level")] public long TrustLevel { get; set; }

        [JsonProperty("deleted_at")] public object DeletedAt { get; set; }

        [JsonProperty("user_deleted")] public bool UserDeleted { get; set; }

        [JsonProperty("edit_reason")] public object EditReason { get; set; }

        [JsonProperty("can_view_edit_history")]
        public bool CanViewEditHistory { get; set; }

        [JsonProperty("wiki")] public bool Wiki { get; set; }

        [JsonProperty("notice_type")] public string NoticeType { get; set; }

        [JsonProperty("notice_args")] public DateTimeOffset NoticeArgs { get; set; }

        [JsonProperty("reviewable_id")] public long ReviewableId { get; set; }

        [JsonProperty("reviewable_score_count")]
        public long ReviewableScoreCount { get; set; }

        [JsonProperty("reviewable_score_pending_count")]
        public long ReviewableScorePendingCount { get; set; }
    }

    public partial class PostActionsSummary
    {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("can_act")] public bool CanAct { get; set; }
    }

    public partial class SuggestedTopic
    {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("fancy_title")] public string FancyTitle { get; set; }

        [JsonProperty("slug")] public string Slug { get; set; }

        [JsonProperty("posts_count")] public long PostsCount { get; set; }

        [JsonProperty("reply_count")] public long ReplyCount { get; set; }

        [JsonProperty("highest_post_number")] public long HighestPostNumber { get; set; }

        [JsonProperty("image_url")] public object ImageUrl { get; set; }

        [JsonProperty("created_at")] public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("last_posted_at")] public DateTimeOffset LastPostedAt { get; set; }

        [JsonProperty("bumped")] public bool Bumped { get; set; }

        [JsonProperty("bumped_at")] public DateTimeOffset BumpedAt { get; set; }

        [JsonProperty("unseen")] public bool Unseen { get; set; }

        [JsonProperty("pinned")] public bool Pinned { get; set; }

        [JsonProperty("unpinned")] public object Unpinned { get; set; }

        [JsonProperty("visible")] public bool Visible { get; set; }

        [JsonProperty("closed")] public bool Closed { get; set; }

        [JsonProperty("archived")] public bool Archived { get; set; }

        [JsonProperty("bookmarked")] public object Bookmarked { get; set; }

        [JsonProperty("liked")] public object Liked { get; set; }

        [JsonProperty("archetype")] public string Archetype { get; set; }

        [JsonProperty("like_count")] public long LikeCount { get; set; }

        [JsonProperty("views")] public long Views { get; set; }

        [JsonProperty("category_id")] public long CategoryId { get; set; }

        [JsonProperty("featured_link")] public object FeaturedLink { get; set; }

        [JsonProperty("posters")] public Poster[] Posters { get; set; }
    }

    public partial class Poster
    {
        [JsonProperty("extras")] public string Extras { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("user")] public CreatedBy User { get; set; }
    }
}