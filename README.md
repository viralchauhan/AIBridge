# AI.Bridge — A Unified AI Wrapper for .NET

AI.Bridge is a modular, dependency-injected wrapper over Microsoft.Extensions.AI that unifies access to multiple AI providers (e.g., OpenAI via GitHub Models endpoint, and Ollama). It exposes simple, consistent services for Chat, Embeddings, Vision, and Vector Search, so you can switch providers or models without changing your app logic.

- Target Framework: .NET 10.0 (`net10.0`)
- Packages: Microsoft.Extensions.AI, Microsoft.Extensions.AI.OpenAI, OllamaSharp, Microsoft.Extensions.VectorData, etc.
- Sample app provided in `Sample/`


## Features

- **Multi-provider support**: OpenAI (via GitHub Models), Ollama
- **Unified services**: `IChatService`, `IEmbeddingService`, `IVisionService`, `IVectorStoreService`
- **Streaming chat**: Token-by-token responses
- **Structured output**: Strongly-typed JSON responses
- **Function calling**: Use `AITool` / `AIFunctionFactory` to enable tool calls
- **Embeddings + Vector search**: In-memory vector store via Microsoft.Extensions.VectorData
- **Provider and model override**: Per-call `WithProvider(...)`, `WithModel(...)`
- **Health checks**: Built-in `AIServiceHealthCheck`


## Repository Structure

- `AI.Bridge/`
  - `AIWrapper.Core/Abstractions/` — Core interfaces (e.g., `IAIService`, `IChatService`, `IEmbeddingService`, `IVisionService`, `IVectorStoreService`, `IAIProvider`).
  - `AIWrapper.Services/` — Concrete services (`AIService`, Chat, Embeddings, Vision, Vector Store).
  - `AIWrapper.Providers/` — Providers (e.g., `OpenAIProvider`, `OllamaProvider`).
  - `AIWrapper.Extensions/` — DI extensions and health check (`ServiceCollectionExtensions`, `AIServiceHealthCheck`).
  - `AI.Bridge.csproj` — Package references and optional web build settings.
- `Sample/` — Console sample showing end-to-end usage and configuration.


## Installation

Since this repository is a library plus a sample, you can:

- Reference the project directly in your solution, or
- Package it and consume via your internal feed.

Required packages are already specified in `AI.Bridge/AI.Bridge.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.AI" Version="9.8.0" />
  <PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="9.8.0-preview.1.25412.6" />
  <PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.8" />
  <PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="9.0.8" />
  <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="10.0.0-rc.1.25451.107" />
  <PackageReference Include="Microsoft.Extensions.Options" Version="10.0.0-rc.1.25451.107" />
  <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="9.0.9" />
  <PackageReference Include="OllamaSharp" Version="5.3.6" />
  <PackageReference Include="System.Numerics.Tensors" Version="9.0.8" />
  <PackageReference Include="Microsoft.Extensions.VectorData.Abstractions" Version="9.7.0" />
  <PackageReference Include="Microsoft.SemanticKernel.Connectors.InMemory" Version="1.64.0-preview" />
</ItemGroup>
```


## Configuration

AI.Bridge is configuration-driven. See the working sample settings in `Sample/appsettings.json`:

```json
{
  "AIWrapper": {
    "DefaultProvider": "OpenAI",
    "Providers": {
      "OpenAI": {
        "Endpoint": "https://models.github.ai/inference",
        "ApiKey": "GitHubModels:Token",
        "Models": {
          "Chat": "openai/gpt-4.1-mini",
          "Embeddings": "openai/text-embedding-3-small"
        }
      },
      "Ollama": {
        "Endpoint": "http://localhost:11434",
        "Models": {
          "Chat": "llama3.2",
          "Embeddings": "all-minilm",
          "Vision": "llava"
        }
      }
    },
    "Services": {
      "Chat": {
        "DefaultMaxTokens": 4000,
        "DefaultTemperature": 0.7,
        "EnableStreaming": true
      },
      "Embeddings": {
        "DefaultDimensions": 384,
        "BatchSize": 100
      },
      "Vision": {
        "MaxImageSize": "5MB",
        "SupportedFormats": ["jpg", "png", "webp"]
      }
    }
  }
}
```

For development, keep your API keys in User Secrets (the sample reads `GitHubModels:Token`):

```bash
# from the Sample project directory
# dotnet user-secrets init (only once)
dotnet user-secrets set "GitHubModels:Token" "YOUR_GITHUB_TOKEN"
```


## Dependency Injection Setup

Use the provided extension in `AIWrapper.Extensions/ServiceCollectionExtensions.cs`:

```csharp
using AI.Bridge.AIWrapper.Extensions;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

// Point to the AIWrapper configuration section
var aiWrapperSection = builder.Configuration.GetSection("AIWrapper");

// Optionally, map your secret token into OpenAI section if using GitHub Models
var githubToken = builder.Configuration["GitHubModels:Token"];
aiWrapperSection.GetSection("Providers:OpenAI:ApiKey").Value = githubToken;

builder.Services.AddAIWrapper(aiWrapperSection);

var host = builder.Build();
var ai = host.Services.GetRequiredService<IAIService>();
```


## Usage Examples

All examples below are runnable in `Sample/Program.cs`.

### Chat Completion

```csharp
var chatResponse = await ai.Chat.CompleteAsync("What is AI? Explain in 20 words.");
Console.WriteLine(chatResponse.Messages.LastOrDefault()?.Text);
```

### Streaming Chat

```csharp
await foreach (var chunk in ai.Chat.CompleteStreamingAsync("Tell me a short joke"))
{
    Console.Write(chunk.Content);
}
Console.WriteLine();
```

### Structured Output

```csharp
public class CarDetails
{
    public required string Condition { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public int Price { get; set; }
    public required string[] Features { get; set; }
    public required string TenWordSummary { get; set; }
}

var prompt = "Convert the car listing to a JSON object matching the CarDetails schema.";
var details = await ai.Chat.CompleteStructuredAsync<CarDetails>(prompt);
```

### Function Calling

```csharp
using Microsoft.Extensions.AI;

var weatherFunction = AIFunctionFactory.Create((string location, string unit) =>
{
    var temperature = Random.Shared.Next(5, 20);
    return $"The weather in {location} is {temperature} degrees {unit}.";
}, name: "get_current_weather", description: "Get current weather by location");

var history = new List<ChatMessage>
{
    new(ChatRole.System, "You are a helpful weather assistant."),
    new(ChatRole.User, "What's the weather like in Istanbul?")
};

var functionResponse = await ai.Chat.CompleteWithFunctionsAsync(history, new[] { weatherFunction });
Console.WriteLine(functionResponse.Messages.LastOrDefault()?.Text);
```

### Embeddings and Vector Search

```csharp
var cat = await ai.Embeddings.GenerateEmbeddingAsync("cat");
var kitten = await ai.Embeddings.GenerateEmbeddingAsync("kitten");
var sim = await ai.Embeddings.CalculateSimilarityAsync(cat, kitten);
Console.WriteLine($"Similarity: {sim:F3}");

public class Movie
{
    [VectorStoreKey]
    public int Key { get; set; }

    [VectorStoreData]
    public required string Title { get; set; }

    [VectorStoreData]
    public required string Description { get; set; }

    [VectorStoreVector(Dimensions: 384, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}

var movies = new List<Movie>
{
    new() { Key = 1, Title = "Lion King", Description = "A young lion's journey" },
    new() { Key = 2, Title = "Inception", Description = "A sci-fi thriller about dreams" }
};

var collection = await ai.VectorStore.GetCollectionAsync<int, Movie>("movies");

foreach (var m in movies)
{
    m.Vector = await ai.Embeddings.GenerateEmbeddingAsync(m.Description);
    await ai.VectorStore.UpsertAsync("movies", m.Key, m);
}

var query = await ai.Embeddings.GenerateEmbeddingAsync("science fiction movie");
var results = await ai.VectorStore.SearchAsync<Movie>("movies", query, top: 2);
foreach (var r in results)
{
    Console.WriteLine($"- {r.Record.Title} (Score: {r.Score:F3})");
}
```

### Vision (image analysis)

```csharp
try
{
    // Provide a valid image file path or byte[] data
    // var vision = await ai.Vision.AnalyzeImageAsync("path/to/image.jpg", "Describe this image");
    // Console.WriteLine(vision.Content);
}
catch (Exception ex)
{
    Console.WriteLine($"Vision not available: {ex.Message}");
}
```

### Switching Provider/Model Per Call

```csharp
var openAiHello = await ai.Chat
    .WithProvider("OpenAI")
    .WithModel("gpt-4")
    .CompleteAsync("Say hello from OpenAI");

var ollamaHello = await ai.Chat
    .WithProvider("Ollama")
    .WithModel("llama3.2")
    .CompleteAsync("Say hello from Ollama");
```


## Health Checks

`AddAIWrapper(...)` registers an `AIServiceHealthCheck` you can attach to ASP.NET Core health checks. In non-web scenarios, it still validates configuration and provider availability during usage.


## Building and Running the Sample

From the repository root:

```bash
# 1) Set your GitHub Models token for development
cd Sample
# dotnet user-secrets init (only once)
dotnet user-secrets set "GitHubModels:Token" "YOUR_GITHUB_TOKEN"

# 2) Run the console sample
cd ..
dotnet run --project Sample
```

If you have Ollama running locally at `http://localhost:11434`, the Ollama examples will work as well (model names must be pulled first, e.g., `ollama pull llama3.2`).


## Supported Providers

- **OpenAI (via GitHub Models endpoint)**
  - Chat, Structured output, Functions, Embeddings, Streaming
- **Ollama**
  - Chat, Embeddings, Vision (models like `llava`), Streaming


## Public Interfaces

Key abstractions are in `AIWrapper.Core/Abstractions/`:

- `IAIService` — Aggregates `Chat`, `Embeddings`, `Vision`, `VectorStore`.
- `IChatService` — Chat completion, streaming, structured output, function calling, `WithProvider`, `WithModel`.
- `IEmbeddingService` — Embedding generation, similarity, `WithProvider`, `WithModel`.
- `IVisionService` — Vision analysis and structured vision results, `WithProvider`, `WithModel`.
- `IVectorStoreService` — Create/get collections, upsert, and search.


## License

This project is licensed under the MIT License. See [`LICENSE`](LICENSE).


## Contributing

- Fork the repository
- Create a feature branch
- Add tests for new functionality (where applicable)
- Submit a pull request
