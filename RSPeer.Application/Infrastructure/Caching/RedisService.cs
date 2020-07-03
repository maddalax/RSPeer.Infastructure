using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RSPeer.Application.Infrastructure.Caching.Base;
using StackExchange.Redis;

namespace RSPeer.Application.Infrastructure.Caching
{
	public class RedisService : IRedisService
	{
		private static ConnectionMultiplexer _connection;

		public RedisService()
		{
			string connection = "localhost";
			var aws = Environment.GetEnvironmentVariable("REDIS_CACHE_URL");
			Console.WriteLine(aws);
			if (!string.IsNullOrEmpty(aws))
			{
				connection = aws;
			}
			else
			{
				var env = Environment.GetEnvironmentVariable("REDIS_URL");
				if (!string.IsNullOrEmpty(env))
				{
					env = env.Replace("rediss://", "").Replace("redis://", "");
					var split = env.Split("@");
					var hostAndPort = split[1];
					var password = split[0].Split(":")[1];
					Console.WriteLine(hostAndPort + " " + password);
					connection = $"{hostAndPort},ssl=false,password={password},abortConnect=false";
				}
			}

			_connection = ConnectionMultiplexer.Connect(connection);
		}

		private ConnectionMultiplexer Connection => _connection;

		public IDatabase GetDatabase()
		{
			return Connection.GetDatabase();
		}

		ConnectionMultiplexer IRedisService.Connection()
		{
			return Connection;
		}

		public async Task<T> Get<T>(string key, Func<Task<T>> orElse = null)
		{
			var value = await GetDatabase().StringGetAsync(key);
			return !value.HasValue ? default : JsonSerializer.Deserialize<T>(value);
		}

		public async Task<T> GetUnsafe<T>(string key)
		{
			var value = await GetDatabase().StringGetAsync(key);
			return !value.HasValue ? default : JsonSerializer.Deserialize<T>(value);
		}

		public async Task<bool?> HasKey(string key)
		{
			return await GetDatabase().KeyExistsAsync(key);
		}

		public async Task<T> GetOrDefault<T>(string key, Func<Task<T>> value, TimeSpan? slidingExpiration = null)
		{
			var cached = await GetDatabase().StringGetAsync(key);
			if (cached.HasValue)
			{
				return JsonSerializer.Deserialize<T>(cached);
			}

			var backup = await value.Invoke();
			await SetJson(key, backup, slidingExpiration);
			return backup;
		}

		public async Task<string> GetString(string key)
		{
			return await GetDatabase().StringGetAsync(key);
		}

		public async Task<string> GetStringUnsafe(string key)
		{
			return await GetDatabase().StringGetAsync(key);
		}

		public async Task<bool> Set(string key, object value, TimeSpan? slidingExpiration = null)
		{
			return await GetDatabase().StringSetAsync(key, value.ToString(),
				slidingExpiration ?? TimeSpan.FromHours(24));
		}

		public async Task<bool> SetJson(string key, object json, TimeSpan? slidingExpiration = null)
		{
			return await Set(key, JsonSerializer.Serialize(json), slidingExpiration);
		}

		public async Task<bool> AddToSet(string key, string value, TimeSpan? expiration = null)
		{
			var db = GetDatabase();
			var added = await db.SetAddAsync(key, value);
			if (expiration.HasValue)
				await db.KeyExpireAsync(key, expiration.Value);
			return added;
		}

		public async Task<HashSet<string>> GetSet(string key)
		{
			var results = await GetDatabase().SetMembersAsync(key);
			return results.Select(w => w.ToString()).ToHashSet();
		}

		public async Task<long> SetRemove(string key, params string[] values)
		{
			return await GetDatabase().SetRemoveAsync(key, values.ToRedisValueArray());
		}

		public async Task<long> SetLength(string key)
		{
			return await GetDatabase().SetLengthAsync(key);
		}

		public async Task<bool> Remove(string key)
		{
			return await GetDatabase().KeyDeleteAsync(key);
		}

		public async Task<long> Increment(string key)
		{
			return await GetDatabase().StringIncrementAsync(key);
		}

		public async Task<long> Decrement(string key)
		{
			return await GetDatabase().StringDecrementAsync(key);
		}

		public async Task<bool> KeyExpire(string key, TimeSpan expiration)
		{
			return await GetDatabase().KeyExpireAsync(key, expiration);
		}

		public async Task<TimeSpan?> TimeToLive(string key)
		{
			return await GetDatabase().KeyTimeToLiveAsync(key);
		}
	}
}
