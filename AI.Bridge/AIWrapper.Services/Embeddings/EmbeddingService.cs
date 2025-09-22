using AI.Bridge.AIWrapper.Core.Abstractions;
using AI.Bridge.AIWrapper.Core.Models;
using Microsoft.Extensions.Options;
using System.Numerics.Tensors;


namespace AI.Bridge.AIWrapper.Services.Embeddings;

public class EmbeddingService : IEmbeddingService
{
    private readonly Dictionary<string, IAIProvider> _providers;
    private readonly IOptionsMonitor<AIServiceOptions> _options;
    private string _currentProvider;
    private string? _currentModel;

    public EmbeddingService(
        IOptionsMonitor<AIServiceOptions> options,
        IEnumerable<IAIProvider> providers)
    {
        _options = options;
        _providers = providers.ToDictionary(p => p.Name, p => p);
        _currentProvider = options.CurrentValue.DefaultProvider;
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
     string text,
     CancellationToken cancellationToken = default)
    {
        var provider = GetCurrentProvider();
        var generator = provider.GetEmbeddingGenerator(_currentModel);
        if (generator == null)
            throw new InvalidOperationException(
                $"Embedding generator not available for provider {_currentProvider}");

        // Use GenerateAsync (takes IEnumerable<string>)
        var result = await generator.GenerateAsync(
            new[] { text }, // single input wrapped in array
            cancellationToken: cancellationToken
        );

        // result is GeneratedEmbeddings<Embedding<float>>
        // Extract the first embedding’s vector
        return result[0].Vector;
    }

    public async Task<IEnumerable<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
    IEnumerable<string> texts,
    CancellationToken cancellationToken = default)
    {
        var provider = GetCurrentProvider();
        var generator = provider.GetEmbeddingGenerator(_currentModel);
        if (generator == null)
            throw new InvalidOperationException(
                $"Embedding generator not available for provider {_currentProvider}");

        // Generate embeddings in batch
        var generated = await generator.GenerateAsync(
            texts,
            cancellationToken: cancellationToken
        );

        // Extract the vectors
        return generated.Select(e => e.Vector);
    }


    public Task<float> CalculateSimilarityAsync(ReadOnlyMemory<float> embedding1, ReadOnlyMemory<float> embedding2, CancellationToken cancellationToken = default)
    {
        var similarity = TensorPrimitives.CosineSimilarity(embedding1.Span, embedding2.Span);
        return Task.FromResult(similarity);
    }

    public IEmbeddingService WithProvider(string providerName)
    {
        _currentProvider = providerName;
        return this;
    }

    public IEmbeddingService WithModel(string modelName)
    {
        _currentModel = modelName;
        return this;
    }

    private IAIProvider GetCurrentProvider()
    {
        if (!_providers.TryGetValue(_currentProvider, out var provider))
            throw new InvalidOperationException($"Provider {_currentProvider} not found");
        return provider;
    }


}
