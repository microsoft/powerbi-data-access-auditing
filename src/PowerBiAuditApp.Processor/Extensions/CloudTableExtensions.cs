using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace PowerBiAuditApp.Processor.Extensions
{
    public static class CloudTableExtensions
    {
        public static async Task<T> GetAsync<T>(this CloudTable cloudTable, string partitionKey, string rowKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, partitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.Equal, rowKey)
                )
            );
            return (await cloudTable.ExecuteQuerySegmentedAsync(query, null)).SingleOrDefault();
        }

        public static async Task UpsertEntityAsync<T>(this CloudTable processRunTable, T entity) where T : ITableEntity, new()
        {
            var upsertOperation = TableOperation.InsertOrReplace(entity);
            await processRunTable.ExecuteAsync(upsertOperation);
        }

        public static async Task DeleteEntityAsync<T>(this CloudTable processRunTable, T entity) where T : ITableEntity, new()
        {
            var upsertOperation = TableOperation.Delete(entity);
            await processRunTable.ExecuteAsync(upsertOperation);
        }


        public static Task<List<T>> GetEntitiesOlderThanAsync<T>(this CloudTable cloudTable, DateTimeOffset date) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(
                TableQuery.GenerateFilterConditionForDate(nameof(ITableEntity.Timestamp), QueryComparisons.LessThan, date)
                );

            return ToList(cloudTable, query);
        }
        public static Task<List<T>> GetEntitiesOlderThanAsync<T>(this CloudTable cloudTable, string partitionKey, DateTimeOffset date) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(
                 TableQuery.CombineFilters(
                     TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, partitionKey),
                     TableOperators.And,
                     TableQuery.GenerateFilterConditionForDate(nameof(ITableEntity.Timestamp), QueryComparisons.LessThan, date)
                 )
                );

            return ToList(cloudTable, query);
        }

        public static Task<List<T>> ToListAsync<T>(this CloudTable cloudTable) where T : ITableEntity, new() => ToList(cloudTable, new TableQuery<T>());

        public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this CloudTable cloudTable, Func<TSource, TKey> keySelector) where TSource : ITableEntity, new()
            => (await cloudTable.ToListAsync<TSource>()).ToDictionary(keySelector);

        private static async Task<List<T>> ToList<T>(CloudTable cloudTable, TableQuery<T> query) where T : ITableEntity, new()
        {
            var list = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                var page = await cloudTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = page.ContinuationToken;
                list.AddRange(page.Results);
            } while (continuationToken != null);

            return list;
        }
    }
}