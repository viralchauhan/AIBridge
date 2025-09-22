using AI.Bridge.AIWrapper.Core.Abstractions;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace AI.Bridge.AIWrapper.Services.VectorStore;

public class VectorStoreService : IVectorStoreService
{
    private readonly InMemoryVectorStore _vectorStore;

    public VectorStoreService()
    {
        _vectorStore = new InMemoryVectorStore();
    }

    public async Task<VectorStoreCollection<TKey, TRecord>> GetCollectionAsync<TKey, TRecord>(string collectionName, VectorStoreCollectionDefinition definition = null)
        where TKey : notnull
        where TRecord : class
    {
        var collection = _vectorStore.GetCollection<TKey, TRecord>(collectionName);
        await collection.EnsureCollectionExistsAsync();
        return collection;
    }

    public async Task<IEnumerable<VectorSearchResult<TRecord>>> SearchAsync<TRecord>(string collectionName, ReadOnlyMemory<float> queryVector, int top = 5)
        where TRecord : class
    {
        var collection = _vectorStore.GetCollection<int, TRecord>(collectionName);
        var searchResults = collection.SearchAsync(queryVector, top: top);

        var results = new List<VectorSearchResult<TRecord>>();
        await foreach (var result in searchResults)
        {
            results.Add(new VectorSearchResult<TRecord>(result.Record, result.Score));
        }
        return results;
    }

    public async Task UpsertAsync<TKey, TRecord>(string collectionName, TKey key, TRecord record)
        where TKey : notnull
        where TRecord : class
    {
        var collection = await GetCollectionAsync<TKey, TRecord>(collectionName);
        await collection.UpsertAsync(record);
    }
}
