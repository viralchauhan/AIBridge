using AI.Bridge.AIWrapper.Core.Abstractions;
using AI.Bridge.AIWrapper.Core.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp;

namespace AI.Bridge.AIWrapper.Providers.Ollama;

public class OllamaProvider : IAIProvider
{
    private readonly Uri _endpoint;


    public string Name => "Ollama";
    public bool SupportsFunctions => false; // Ollama has limited function support
    public bool SupportsVision => true;
    public bool SupportsStreaming => true;

    private readonly OllamaApiClient _client;
    private readonly ProviderConfiguration _config;

    public OllamaProvider(IOptionsMonitor<AIServiceOptions> options)
    {
        _config = options.CurrentValue.Providers["Ollama"];
        _endpoint = new Uri(_config.Endpoint ?? "http://localhost:11434");
        _client = new OllamaApiClient(_endpoint);
    }

    public IChatClient? GetChatClient(string? modelName = null)
    {
        var model = modelName ?? _config.Models.Chat ?? "llama3.2";
        return new OllamaApiClient(_endpoint, model);
    }

    public IEmbeddingGenerator<string, Embedding<float>>? GetEmbeddingGenerator(string? modelName = null)
    {
        var model = modelName ?? _config.Models.Embeddings ?? "all-minilm";
        return new OllamaApiClient(_endpoint, model);
    }
}
