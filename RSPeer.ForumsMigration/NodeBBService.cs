using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RSPeer.Application.Features.Forums.Discourse.Commands;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Domain.Entities;

namespace RSPeer.ForumsMigration
{
    public class NodeBbService
    {
        private readonly IMongoCollection<BsonDocument> objects;
        private readonly IMediator _mediator;
        private static readonly ConcurrentDictionary<int, User> userMap = new ConcurrentDictionary<int, User>();

        public NodeBbService(IMediator mediator)
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            var database = mongoClient.GetDatabase("nodebb");
            objects = database.GetCollection<BsonDocument>("objects");
            _mediator = mediator;
        }

        public async Task<IEnumerable<NodeBBTopicId>> GetTopicIds(bool ignorePorted = true)
        {
            return await objects.Find(w => w["_key"] == "topics:tid" && w["ported"] != ignorePorted)
                .As<NodeBBTopicId>().ToListAsync();
        }

        public async Task<NodeBBTopic> GetTopic(int id)
        {
            return await objects.Find(w => w["_key"] == $"topic:{id}" && w["deleted"] == 0)
                .As<NodeBBTopic>().FirstOrDefaultAsync();
        }

        public async Task<NodeBBPost> GetPost(int id)
        {
            return await objects.Find(w => w["_key"] == $"post:{id}" && w["deleted"] == 0)
                .As<NodeBBPost>().FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<NodeBBPost>> GetPostsForTopic(int id)
        {
            return await objects.Find(w => w["tid"] == id && w["deleted"] == 0)
                .As<NodeBBPost>().Skip(2).ToListAsync();
        }

        public async Task<User> GetUser(int id)
        {
            if (userMap.ContainsKey(id))
            {
                userMap.TryGetValue(id, out var cached);
                return cached;
            }

            var nodeBbUser = await objects.Find(w => w["_key"] == $"user:{id}")
                .As<NodeBBUser>().FirstOrDefaultAsync();
            if (nodeBbUser == null)
            {
                return null;
            }

            if (nodeBbUser.Email == "maddev@rspeer.org")
            {
                var me = await _mediator.Send(new GetUserByEmailQuery {Email = nodeBbUser.Email});
                userMap.TryAdd(id, me);
                return me;
            }

            var user = await _mediator.Send(new SyncDiscourseUsersCommand
            {
                Email = nodeBbUser.Email
            });
            if (user == null)
            {
                Console.WriteLine(nodeBbUser.Email + " not found in RSPeer database.");
            }

            if (user != null)
            {
                userMap.TryAdd(id, user);
            }

            return user;
        }
    }

    [BsonIgnoreExtraElements]
    public class NodeBBTopicId
    {
        [BsonElement("value")]
        public int Value { get; set; }

        [BsonElement("ported")]
        public bool Ported { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class NodeBBTopic
    {
        [BsonElement("cid")]
        public int Category { get; set; }

        [BsonElement("deleted")]
        public int Deleted { get; set; }

        [BsonElement("locked")]
        public int Locked { get; set; }

        [BsonElement("mainPid")]
        public int PostId { get; set; }

        [BsonElement("tid")]
        public int TopicId { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("uid")]
        public int UserId { get; set; }

        [BsonElement("timestamp")]
        public double Timestamp { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class NodeBBPost
    {
        [BsonElement("deleted")]
        public int Deleted { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("pid")]
        public int PostId { get; set; }

        [BsonElement("tid")]
        public int TopicId { get; set; }

        [BsonElement("timestamp")]
        public double Timestamp { get; set; }

        [BsonElement("uid")]
        public int UserId { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class NodeBBUser
    {
        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("userslug")]
        public string UserSlug { get; set; }
    }
}