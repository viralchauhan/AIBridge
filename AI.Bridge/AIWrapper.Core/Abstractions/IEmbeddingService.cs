using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Bridge.AIWrapper.Core.Abstractions;

public interface IEmbeddingService
{
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);
    Task<float> CalculateSimilarityAsync(ReadOnlyMemory<float> embedding1, ReadOnlyMemory<float> embedding2, CancellationToken cancellationToken = default);
    IEmbeddingService WithProvider(string providerName);
    IEmbeddingService WithModel(string modelName);
}
