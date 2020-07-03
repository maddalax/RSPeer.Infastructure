using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSPeer.Common.Linq
{
	public static class QueryableExtensions
	{
		public static async Task ForEachBulkAsync<T>(this IQueryable<T> query, int take, Func<List<T>, Task> callback)
		{
			var skip = 0;
			while (true)
			{
				var result = query.Skip(skip).Take(take).ToList();
				if (result.Count == 0)
				{
					break;
				}

				await callback(result);
				skip += result.Count;
			}
		}
	}
}