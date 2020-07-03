using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RSPeer.Application.Infrastructure.Caching.Base
{
	public interface IRedisService
	{
		ConnectionMultiplexer Connection();
		Task<T> Get<T>(string key,  Func<Task<T>> orElse = null);
		Task<T> GetUnsafe<T>(string key);
		Task<bool?> HasKey(string key);
		Task<T> GetOrDefault<T>(string key, Func<Task<T>> value, TimeSpan? slidingExpiration = null);
		Task<string> GetString(string key);
		Task<string> GetStringUnsafe(string key);
		Task<bool> Set(string key, object value, TimeSpan? slidingExpiration = null);
		Task<bool> SetJson(string key, object json, TimeSpan? slidingExpiration = null);
		Task<bool> AddToSet(string key, string value, TimeSpan? slidingExpiration = null);
		Task<HashSet<string>> GetSet(string key);
		Task<long> SetRemove(string key, params string[] values);
		Task<long> SetLength(string key);
		Task<bool> Remove(string key);
		Task<long> Increment(string key);
		Task<long> Decrement(string key);
		Task<bool> KeyExpire(string key, TimeSpan expiration);
		Task<TimeSpan?> TimeToLive(string key);
		IDatabase GetDatabase();
	}
}