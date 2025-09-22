using AI.Bridge.AIWrapper.Core.Models;
using Microsoft.Extensions.AI;

namespace AI.Bridge.AIWrapper.Core.Abstractions;

public interface IChatService
{
    Task<ChatResponse> CompleteAsync(string messages, CancellationToken cancellationToken = default);
    Task<ChatResponse> CompleteAsync(List<ChatMessage> messages, CancellationToken cancellationToken = default);
    IAsyncEnumerable<StreamingChatResponse> CompleteStreamingAsync(string prompt, CancellationToken cancellationToken = default);
    Task<T> CompleteStructuredAsync<T>(string prompt, CancellationToken cancellationToken = default) where T : class;
    Task<ChatResponse> CompleteWithFunctionsAsync(List<ChatMessage> messages, IEnumerable<AITool> functions, CancellationToken cancellationToken = default);
    IChatService WithProvider(string providerName);
    IChatService WithModel(string modelName);
    IChatService WithOptions(ChatServiceOptions options);
}
