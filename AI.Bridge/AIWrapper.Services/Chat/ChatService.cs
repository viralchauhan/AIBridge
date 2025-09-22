using AI.Bridge.AIWrapper.Core.Abstractions;
using AI.Bridge.AIWrapper.Core.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp.Models.Chat;


namespace AI.Bridge.AIWrapper.Services.Chat;

public class ChatService : IChatService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<AIServiceOptions> _options;
    private readonly Dictionary<string, IAIProvider> _providers;
    private string _currentProvider;
    private string? _currentModel;
    private ChatServiceOptions? _currentOptions;

    public ChatService(
        IServiceProvider serviceProvider,
        IOptionsMonitor<AIServiceOptions> options,
        IEnumerable<IAIProvider> providers)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _providers = providers.ToDictionary(p => p.Name, p => p);
        _currentProvider = options.CurrentValue.DefaultProvider;
    }

    public async Task<ChatResponse> CompleteAsync(string messages, CancellationToken cancellationToken = default)
    {
        var provider = GetCurrentProvider();
        var client = provider.GetChatClient(_currentModel);
        if (client == null) throw new InvalidOperationException($"Chat client not available for provider {_currentProvider}");

        return await client.GetResponseAsync(messages, options: null, cancellationToken);
    }

    public async Task<ChatResponse> CompleteAsync(List<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var provider = GetCurrentProvider();
        var client = provider.GetChatClient(_currentModel);
        if (client == null) throw new InvalidOperationException($"Chat client not available for provider {_currentProvider}");

        return await client.GetResponseAsync(messages, options: null, cancellationToken);
    }

    public async IAsyncEnumerable<StreamingChatResponse> CompleteStreamingAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var provider = GetCurrentProvider();
        var client = provider.GetChatClient(_currentModel);
        if (client == null) throw new InvalidOperationException($"Chat client not available for provider {_currentProvider}");

        await foreach (var item in client.GetStreamingResponseAsync(prompt, options: null, cancellationToken))
        {
            yield return new StreamingChatResponse(item.Text, true);
        }
    }

    public async Task<T> CompleteStructuredAsync<T>(string prompt,bool isJsonFormatSchemaResponse = false, CancellationToken cancellationToken = default) where T : class
    {
        var provider = GetCurrentProvider();
        var client = provider.GetChatClient(_currentModel);
        if (client == null) throw new InvalidOperationException($"Chat client not available for provider {_currentProvider}");
        
        var messages = new[] { new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, prompt) };

        var response = await client.GetResponseAsync<T>(messages, options: null, isJsonFormatSchemaResponse, cancellationToken);
        if (response.TryGetResult(out var result))
        {
            return result;
        }
        throw new InvalidOperationException("Failed to parse structured response");
    }

    public async Task<ChatResponse> CompleteWithFunctionsAsync(List<ChatMessage> messages, IEnumerable<AITool> functions, CancellationToken cancellationToken = default)
    {
        var provider = GetCurrentProvider();
        if (!provider.SupportsFunctions)
            throw new NotSupportedException($"Provider {_currentProvider} does not support function calling");

        var client = provider.GetChatClient(_currentModel);
        if (client == null) throw new InvalidOperationException($"Chat client not available for provider {_currentProvider}");

        var options = new ChatOptions { Tools = functions.ToList() };
        return await client.GetResponseAsync(messages, options, cancellationToken);
    }

    public IChatService WithProvider(string providerName)
    {
        _currentProvider = providerName;
        return this;
    }

    public IChatService WithModel(string modelName)
    {
        _currentModel = modelName;
        return this;
    }

    public IChatService WithOptions(ChatServiceOptions options)
    {
        _currentOptions = options;
        return this;
    }

    private IAIProvider GetCurrentProvider()
    {
        if (!_providers.TryGetValue(_currentProvider, out var provider))
            throw new InvalidOperationException($"Provider {_currentProvider} not found");
        return provider;
    }

    public async Task<T> CompleteStructuredAsync<T>(string prompt, CancellationToken cancellationToken = default) where T : class
    {
        var provider = GetCurrentProvider();
        var client = provider.GetChatClient(_currentModel);
        var messages = new[] { new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, prompt) };

        if (client == null)
            throw new InvalidOperationException($"Chat client not available for provider {_currentProvider}");

        var response = await client.GetResponseAsync<T>(messages, options: null, useJsonSchemaResponseFormat: true, cancellationToken);

        // Extract the actual object
        return response.Result ?? throw new InvalidOperationException("No response returned from AI.");
    }
}
