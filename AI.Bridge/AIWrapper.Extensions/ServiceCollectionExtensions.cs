using AI.Bridge.AIWrapper.Core.Abstractions;
using AI.Bridge.AIWrapper.Core.Models;
using AI.Bridge.AIWrapper.Providers.Ollama;
using AI.Bridge.AIWrapper.Providers.OpenAI;
using AI.Bridge.AIWrapper.Services;
using AI.Bridge.AIWrapper.Services.Chat;
using AI.Bridge.AIWrapper.Services.Embeddings;
using AI.Bridge.AIWrapper.Services.VectorStore;
using AI.Bridge.AIWrapper.Services.Vision;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;



namespace AI.Bridge.AIWrapper.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAIWrapper(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddAIWrapper(configuration.GetSection("AIWrapper"));
    }

    public static IServiceCollection AddAIWrapper(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        // Configure options
        services.Configure<AIServiceOptions>(configurationSection);


        // Register core services
        services.AddSingleton<IAIService, AIService>();
        services.AddSingleton<IChatService, ChatService>();
        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        services.AddSingleton<IVisionService, VisionService>();
        services.AddSingleton<IVectorStoreService, VectorStoreService>();

        // Register providers
        services.AddSingleton<IAIProvider, OpenAIProvider>();
        services.AddSingleton<IAIProvider, OllamaProvider>();

        // Add health checks
        services.AddHealthChecks()
            .AddCheck<AIServiceHealthCheck>("ai-service");

        return services;
    }

    public static IServiceCollection AddAIWrapper(this IServiceCollection services, Action<AIServiceOptions> configure)
    {
        services.Configure(configure);

        // Register core services
        services.AddSingleton<IAIService, AIService>();
        services.AddSingleton<IChatService, ChatService>();
        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        services.AddSingleton<IVisionService, VisionService>();
        services.AddSingleton<IVectorStoreService, VectorStoreService>();

        // Register providers
        services.AddSingleton<IAIProvider, OpenAIProvider>();
        services.AddSingleton<IAIProvider, OllamaProvider>();

        return services;
    }
}

