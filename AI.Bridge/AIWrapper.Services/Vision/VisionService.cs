using AI.Bridge.AIWrapper.Core.Abstractions;
using AI.Bridge.AIWrapper.Core.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace AI.Bridge.AIWrapper.Services.Vision;

public class VisionService : IVisionService
{
    private readonly Dictionary<string, IAIProvider> _providers;
    private readonly IOptionsMonitor<AIServiceOptions> _options;
    private string _currentProvider;
    private string? _currentModel;

    public VisionService(
        IOptionsMonitor<AIServiceOptions> options,
        IEnumerable<IAIProvider> providers)
    {
        _options = options;
        _providers = providers.ToDictionary(p => p.Name, p => p);
        _currentProvider = options.CurrentValue.DefaultProvider;
    }

    public async Task<VisionResponse> AnalyzeImageAsync(string imagePath, string prompt, CancellationToken cancellationToken = default)
    {
        var imageBytes = await File.ReadAllBytesAsync(imagePath, cancellationToken);
        var mimeType = GetMimeTypeFromPath(imagePath);
        return await AnalyzeImageAsync(imageBytes, prompt, mimeType, cancellationToken);
    }

    public async Task<VisionResponse> AnalyzeImageAsync(byte[] imageData, string prompt, string mimeType = "image/png", CancellationToken cancellationToken = default)
    {
        var provider = GetCurrentProvider();
        if (!provider.SupportsVision)
            throw new NotSupportedException($"Provider {_currentProvider} does not support vision");

        var client = provider.GetChatClient(_currentModel);
        if (client == null) throw new InvalidOperationException($"Chat client not available for provider {_currentProvider}");

        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, prompt),
            new(ChatRole.User, [new DataContent(imageData, mimeType)])
        };

        var response = await client.GetResponseAsync(messages, null, cancellationToken);
        return new VisionResponse(response.Messages.LastOrDefault()?.Text ?? string.Empty);
    }

    public async Task<T> AnalyzeImageStructuredAsync<T>(string imagePath, string prompt, bool jsonStructureResponce = false,CancellationToken cancellationToken = default) where T : class
    {
        var imageBytes = await File.ReadAllBytesAsync(imagePath, cancellationToken);
        var mimeType = GetMimeTypeFromPath(imagePath);
        return await AnalyzeImageStructuredAsync<T>(imageBytes, prompt, jsonStructureResponce , mimeType, cancellationToken);
    }

    public async Task<T> AnalyzeImageStructuredAsync<T>(byte[] imageData, string prompt, bool isJsonStructureResponce = false, string mimeType = "image/png", CancellationToken cancellationToken = default) where T : class
    {
        var provider = GetCurrentProvider();
        if (!provider.SupportsVision)
            throw new NotSupportedException($"Provider {_currentProvider} does not support vision");

        var client = provider.GetChatClient(_currentModel);
        if (client == null) throw new InvalidOperationException($"Chat client not available for provider {_currentProvider}");

        var message = new ChatMessage(ChatRole.User, prompt);
        message.Contents.Add(new DataContent(imageData, mimeType));

        var response = await client.GetResponseAsync<T>([message], null, isJsonStructureResponce , cancellationToken);
        if (response.TryGetResult(out var result))
        {
            return result;
        }
        throw new InvalidOperationException("Failed to parse structured response");
    }

    public IVisionService WithProvider(string providerName)
    {
        _currentProvider = providerName;
        return this;
    }

    public IVisionService WithModel(string modelName)
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

    private static string GetMimeTypeFromPath(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/png"
        };
    }
}