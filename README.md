# Generic AI Wrapper for .NET

[![NuGet](https://img.shields.io/nuget/v/Microsoft.Extensions.AI?label=NuGet)](https://www.nuget.org/packages/Microsoft.Extensions.AI/)  
[![Build Status](https://img.shields.io/github/actions/workflow/status/yourusername/GenericAI/dotnet.yml?branch=main)](https://github.com/yourusername/GenericAI/actions)  
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A comprehensive, configurable AI wrapper library for .NET 10 with C# 14 that provides unified access to multiple AI providers (OpenAI, GitHub Models, Ollama) with support for:

- **Text Completion** (streaming and non-streaming)
- **Function Calling** 
- **Text Analysis** (sentiment, classification, summarization)
- **Structured Output**
- **Image Analysis**
- **Vector Embeddings & Search**
- **Chat Bots**

## Features

✅ **Multi-Provider Support**: OpenAI, GitHub Models, Ollama  
✅ **Unified Interface**: Same code works with different providers  
✅ **Dependency Injection**: Built-in DI support  
✅ **Configuration-Driven**: Easy setup via appsettings.json or user secrets  
✅ **Vector Search**: In-memory vector store with similarity search  
✅ **Streaming Support**: Real-time response streaming  
✅ **Image Analysis**: Vision capabilities for supported models  
✅ **Type-Safe**: Strongly typed with C# 14 features  
✅ **Extensible**: Easy to add new providers and capabilities

## Quick Start

### 1. Installation

```xml
<!-- Add to your .csproj -->
<PackageReference Include="Microsoft.Extensions.AI" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.AI.Ollama" Version="9.0.0" />
<PackageReference Include="OpenAI" Version="2.1.0" />
<PackageReference Include="OllamaSharp" Version="3.0.8" />
```

### 2. Setup with Dependency Injection

```csharp
using GenericAI.Builder;

var services = new ServiceCollection();

// Option 1: GitHub Models (like your examples)
services.AddGenericAI(builder => builder
    .WithGitHubModels(
        token: "your-github-token",
        chatModel: "gpt-4o-mini",
        embeddingModel: "text-embedding-3-small"
    ));

// Option 2: Ollama
services.AddGenericAI(builder => builder
    .WithOllama(
        baseUrl: "http://localhost:11434",
        chatModel: "llama3.2",
        embeddingModel: "all-minilm"
    ));

var serviceProvider = services.BuildServiceProvider();
var aiService = serviceProvider.GetRequiredService<IGenericAiService>();
```

### 3. Basic Usage Examples

```csharp
// Simple text completion
var response = await aiService.QuickCompletionAsync("What is AI?");
Console.WriteLine(response.Result);

// Streaming completion
await foreach (var chunk in aiService.QuickStreamingCompletionAsync("Explain machine learning"))
{
    Console.Write(chunk);
}

// Sentiment analysis
var sentiment = await aiService.TextAnalyzer.AnalyzeSentimentAsync("I love this product!");
Console.WriteLine($"Sentiment: {sentiment.Result}"); // "Positive"

// Text classification
var categories = new[] { "complaint", "suggestion", "praise", "other" };
var classification = await aiService.TextAnalyzer.ClassifyTextAsync("You should add dark mode", categories);
Console.WriteLine($"Category: {classification.Result}"); // "suggestion"
```

## Configuration

### User Secrets (Recommended for Development)
```bash
dotnet user-secrets set "GitHubModels:Token" "your-github-token"
dotnet user-secrets set "AI:Provider" "OpenAI"
dotnet user-secrets set "AI:ChatModel" "gpt-4o-mini"
```

### appsettings.json
```json
{
  "AI": {
    "Provider": "OpenAI",
    "ApiKey": "your-api-key",
    "BaseUrl": "https://api.openai.com",
    "ChatModel": "gpt-4",
    "EmbeddingModel": "text-embedding-3-small",
    "EmbeddingDimensions": "1536"
  }
}
```

## Advanced Usage

### Chat Bot with Memory
```csharp
var chatBot = aiService.CreateChatBot("You are a helpful hiking guide.");

var response1 = await chatBot.SendMessageAsync("Hello!");
var response2 = await chatBot.SendMessageAsync("I want to hike in Seattle");
// Chat history is automatically maintained
```

### Structured Output
```csharp
public record CarDetails
{
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public int Price { get; set; }
}

var result = await aiService.TextAnalyzer.ExtractStructuredDataAsync<CarDetails>(
    "2019 Toyota Camry for $18,000"
);
```

### Vector Search
```csharp
// Define your domain model
public record Movie : VectorRecord<int>
{
    public required string Genre { get; init; }
    public required string Director { get; init; }
    public int Year { get; init; }
}

// Create vector search service
var vectorSearch = aiService.CreateVectorSearchService<int, Movie>("movies");

// Add items
var movie = new Movie
{
    Key = 1,
    Title = "Inception",
    Content = "A sci-fi thriller about dream manipulation...",
    Genre = "Sci-Fi",
    Director = "Christopher Nolan",
    Year = 2010
};
await vectorSearch.AddItemAsync(movie);

// Search by text
var results = await vectorSearch.SearchByTextAsync("space movies", limit: 5);
foreach (var result in results)
{
    Console.WriteLine($"{result.Record.Title} (Score: {result.Score:F2})");
}
```

### Image Analysis
```csharp
var imageData = await File.ReadAllBytesAsync("image.jpg");

// Simple description
var description = await aiService.ImageAnalyzer.DescribeImageAsync(
    imageData, "image/jpeg", "Describe this image");

// Structured analysis
var analysis = await aiService.ImageAnalyzer.AnalyzeImageStructuredAsync<TrafficData>(
    imageData, "image/jpeg", "Count the vehicles in this traffic image");
```

### Function Calling
```csharp
var weatherFunction = AIFunctionFactory.Create((string location, string unit) =>
{
    // Your weather API logic here
    return $"The weather in {location} is sunny, 25°C";
}, "get_weather", "Get current weather");

var functions = new[] { weatherFunction };
var response = await aiProvider.InvokeFunctionAsync(
    "What's the weather in Istanbul?", 
    functions
);
```

## ASP.NET Core Integration

```csharp
// Program.cs
builder.Services.AddGenericAI(aiBuilder => aiBuilder
    .WithConfiguration(builder.Configuration));

// Controller
[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IGenericAiService _aiService;

    public AiController(IGenericAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("completion")]
    public async Task<IActionResult> GetCompletion([FromBody] CompletionRequest request)
    {
        var response = await _aiService.QuickCompletionAsync(request.Prompt);
        return response.IsSuccess 
            ? Ok(response.Result) 
            : BadRequest(response.ErrorMessage);
    }

    [HttpPost("sentiment")]
    public async Task<IActionResult> AnalyzeSentiment([FromBody] TextRequest request)
    {
        var response = await _aiService.TextAnalyzer.AnalyzeSentimentAsync(request.Text);
        return response.IsSuccess 
            ? Ok(new { sentiment = response.Result }) 
            : BadRequest(response.ErrorMessage);
    }
}
```

## Supported Providers

### OpenAI / GitHub Models
- Text completion and chat
- Function calling
- Image analysis (with vision models)
- Text embeddings
- Streaming responses

### Ollama
- Text completion and chat  
- Image analysis (with vision models like llava)
- Text embeddings
- Streaming responses
- Local deployment

## Extension Points

### Adding Custom Providers
```csharp
public class CustomAiProvider : IAiProvider
{
    // Implement the interface methods
    public async Task<string> GetCompletionAsync(string prompt, CancellationToken cancellationToken = default)
    {
        // Your custom implementation
    }
    // ... other methods
}

// Register in DI
services.AddSingleton<IAiProvider, CustomAiProvider>();
```

### Custom Vector Stores
```csharp
services.AddSingleton<IVectorStore>(provider => 
    new YourCustomVectorStore(connectionString));
```

## Error Handling

All operations return `AiResponse<T>` with built-in success/failure handling:

```csharp
var response = await aiService.QuickCompletionAsync("Hello");

if (response.IsSuccess)
{
    Console.WriteLine($"Result: {response.Result}");
    Console.WriteLine($"Tokens: {response.InputTokens}/{response.OutputTokens}");
}
else
{
    Console.WriteLine($"Error: {response.ErrorMessage}");
}
```

## Performance Considerations

- **Streaming**: Use streaming for long responses to improve perceived performance
- **Caching**: Vector embeddings are computed once and reused
- **Batch Operations**: Use `GenerateEmbeddingsAsync` for multiple texts
- **Connection Pooling**: HTTP clients are reused across requests

## License

This is a sample implementation. Adapt the license as needed for your project.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Submit a pull request

## Changelog

### v1.0.0
- Initial release
- Support for OpenAI, GitHub Models, and Ollama
- Text completion, analysis, and vector search
- Image analysis capabilities
- Dependency injection integration
