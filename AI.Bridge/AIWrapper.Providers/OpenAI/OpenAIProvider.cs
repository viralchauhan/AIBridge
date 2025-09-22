using AI.Bridge.AIWrapper.Core.Abstractions;
using AI.Bridge.AIWrapper.Core.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Text;

namespace AI.Bridge.AIWrapper.Providers.OpenAI;

public class OpenAIProvider : IAIProvider
{
    public string Name => "OpenAI";
    public bool SupportsFunctions => true;
    public bool SupportsVision => true;
    public bool SupportsStreaming => true;

    private readonly OpenAIClient _client;
    private readonly ProviderConfiguration _config;

    public OpenAIProvider(IOptionsMonitor<AIServiceOptions> options)
    {
        _config = options.CurrentValue.Providers["OpenAI"];
        var credential = new ApiKeyCredential(_config.ApiKey ?? throw new ArgumentNullException("OpenAI ApiKey"));

        var clientOptions = new OpenAIClientOptions();

        if (!string.IsNullOrEmpty(_config.Endpoint))
        {
            clientOptions.Endpoint = new Uri(_config.Endpoint);
        }

        _client = new OpenAIClient(credential, clientOptions);
    }

    public IChatClient? GetChatClient(string? modelName = null)
    {
        var model = modelName ?? _config.Models.Chat ?? "gpt-4";
        return _client.GetChatClient(model).AsIChatClient();
    }

    public IEmbeddingGenerator<string, Embedding<float>>? GetEmbeddingGenerator(string? modelName = null)
    {
        var model = modelName ?? _config.Models.Embeddings ?? "text-embedding-3-small";
        return _client.GetEmbeddingClient(model).AsIEmbeddingGenerator();
    }
}
