using Microsoft.Extensions.AI;

namespace AI.Bridge.AIWrapper.Core.Abstractions;

public interface IAIProvider
{
    string Name { get; }
    IChatClient? GetChatClient(string? modelName = null);
    IEmbeddingGenerator<string, Embedding<float>>? GetEmbeddingGenerator(string? modelName = null);
    bool SupportsFunctions { get; }
    bool SupportsVision { get; }
    bool SupportsStreaming { get; }
}
