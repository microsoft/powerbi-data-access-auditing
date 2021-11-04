using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;

namespace PowerBiAuditApp.Processor.Extensions;

public static class AsyncPageableExtensions
{
    public static async Task<T> FirstOrDefaultAsync<T>(this AsyncPageable<T> pageable)
    {
        var enumerator = pageable.GetAsyncEnumerator();
        await enumerator.MoveNextAsync();
        return enumerator.Current;
    }

    public static async Task<IList<T>> ToListAsync<T>(this AsyncPageable<T> pageable)
    {
        var list = new List<T>();

        await foreach (var item in pageable)
        {
            list.Add(item);
        }

        return list;
    }
    public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this AsyncPageable<TSource> pageable, Func<TSource, TKey> keySelector)
    {
        var list = new List<TSource>();

        await foreach (var item in pageable)
        {
            list.Add(item);
        }

        return list.ToDictionary(keySelector);
    }
}