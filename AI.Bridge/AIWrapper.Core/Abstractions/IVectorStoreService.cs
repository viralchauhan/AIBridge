using Microsoft.Extensions.VectorData;


namespace AI.Bridge.AIWrapper.Core.Abstractions;

public interface IVectorStoreService
{
    Task<VectorStoreCollection<TKey, TRecord>> GetCollectionAsync<TKey, TRecord>(
        string collectionName,
        VectorStoreCollectionDefinition definition = default)
        where TKey : notnull
        where TRecord : class;

    Task<IEnumerable<VectorSearchResult<TRecord>>> SearchAsync<TRecord>(string collectionName, ReadOnlyMemory<float> queryVector, int top = 5)
        where TRecord : class;
    Task UpsertAsync<TKey, TRecord>(string collectionName, TKey key, TRecord record)
        where TKey : notnull
        where TRecord : class;
}
