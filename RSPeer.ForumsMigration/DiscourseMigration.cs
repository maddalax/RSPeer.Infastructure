using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RSPeer.Application.Features.Forums.Discourse.Commands;
using RSPeer.Common.File;
using RSPeer.Common.Linq;
using RSPeer.Domain.Entities;
using RSPeer.Domain.Entities.Discourse;
using RSPeer.Persistence;

namespace RSPeer.ForumsMigration
{
    public class DiscourseMigration
    {
        private readonly DiscourseService _discourse;
        private readonly RsPeerContext _db;
        private readonly NodeBbService _nodebb;
        private readonly IMediator _mediator;
        private readonly IServiceScopeFactory _factory;
        private HashSet<string> added = new HashSet<string>();

        public DiscourseMigration(DiscourseService discourse, RsPeerContext db, NodeBbService nodebb, IMediator mediator, IServiceScopeFactory factory)
        {
            _discourse = discourse;
            _db = db;
            _nodebb = nodebb;
            _mediator = mediator;
            _factory = factory;
        }

        public async Task Execute()
        {
            if (!File.Exists("topics_created.txt"))
            {
                var stream = File.Create("topics_created.txt");
                stream.Dispose();
            }
            var ids = File.ReadAllLines("topics_created.txt").Select(w => int.Parse(w.Trim())).ToHashSet();
            Console.WriteLine(JsonConvert.SerializeObject(ids));
            var topicIds = await _nodebb.GetTopicIds();
            topicIds = topicIds.Where(w => !ids.Contains(w.Value));
            await topicIds.ForEachAsync(8, async id =>
            {
                using (var scope = _factory.CreateScope())
                {
                    var nodeBbService = scope.ServiceProvider.GetService<NodeBbService>();
                    var discourseService = scope.ServiceProvider.GetService<DiscourseService>();
                    var db = scope.ServiceProvider.GetService<RsPeerContext>();
                    var topic = await nodeBbService.GetTopic(id.Value);
                    if (topic != null)
                    {
                        try
                        {
                            await OnTopic(topic, nodeBbService, discourseService, db);
                        }
                        catch (Exception e)
                        {
                            if (e.ToString().Contains("Title has already been used"))
                            {
                                Console.WriteLine("Already created skipping.");
                                File.AppendAllText("topics_created.txt", $"{id.Value}{Environment.NewLine}");
                                return;
                            }
                            Console.WriteLine(e);
                        }
                    }
                }
            });
        }

        private async Task OnTopic(NodeBBTopic topic, NodeBbService nodebb, DiscourseService discouse, RsPeerContext db)
        {
            var post = await nodebb.GetPost(topic.PostId);
            if (post == null)
            {
                return;
            }

            var user = await nodebb.GetUser(topic.UserId);

            if (user == null)
            {
                return;
            }

            var createdAt = DateTimeOffset.FromUnixTimeMilliseconds((long) topic.Timestamp).ToString("O");

            Console.WriteLine("Creating: " + topic.Title + " " + createdAt);

            var topicId = await discouse.CreateTopic(new DiscourseTopicRequest
            {
                Content = post.Content,
                Category = discouse.GetCategory(topic.Category),
                CreatedAt = createdAt,
                Title = topic.Title
            }, user);

            Console.WriteLine("Updating forum thread with nodebb topic id: " + topic.TopicId);
            var sql = $"UPDATE scripts SET discoursethread = {topicId} where forumthread like '%/topic/{topic.TopicId}/%'";

            var update = await db.Database.GetDbConnection().ExecuteAsync(sql);
            Console.WriteLine(update);

            File.AppendAllText("topics_created.txt", $"{topic.TopicId}{Environment.NewLine}");

            await CreatePostsForTopic(topic.TopicId, topicId, nodebb, discouse);
        }

        private async Task CreatePostsForTopic(int nodeTopicId, int newTopicId, NodeBbService nodebb, DiscourseService discourse)
        {
            var posts = await nodebb.GetPostsForTopic(nodeTopicId);
            foreach (var post in posts)
            {
                try
                {
                    var createdAt = DateTimeOffset.FromUnixTimeMilliseconds((long) post.Timestamp).ToString("O");
                    var created = await discourse.CreatePost(new DiscoursePostRequest
                    {
                        TopicId = newTopicId,
                        CreatedAt = createdAt,
                        Content = post.Content
                    }, await nodebb.GetUser(post.UserId));
                    Console.WriteLine($"Created post {created} for topic {nodeTopicId}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    var user = await nodebb.GetUser(post.UserId);
                    Console.WriteLine($"Failed to create post: {post.Content} with user: " + user?.Email);
                }
            }
        }
    }
}