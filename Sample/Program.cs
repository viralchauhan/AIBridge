

using AI.Bridge.AIWrapper.Core.Abstractions;
using AI.Bridge.AIWrapper.Extensions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.VectorData;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory) // ensures relative path works
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>() // for secrets
    .AddEnvironmentVariables(); // optional

var configuration = builder.Configuration;

var openAiApiKey = configuration["GitHubModels:Token"];

var aiWrapperSection = configuration.GetSection("AIWrapper");

// Optional: replace placeholder with actual key
aiWrapperSection.GetSection("Providers:OpenAI:ApiKey").Value = openAiApiKey;

builder.Services.AddAIWrapper(aiWrapperSection);

var host = builder.Build();

var aiService = host.Services.GetRequiredService<IAIService>();

// =============================================================================
// Example 1: Basic Chat Completion
// =============================================================================

Console.WriteLine("=== Basic Chat Completion ===");

var chatResponse = await aiService.Chat.CompleteAsync("What is AI? Explain in 20 words.");

Console.WriteLine($"Response: {chatResponse.Messages.LastOrDefault()?.Text}");

Console.WriteLine();


// =============================================================================
// Example 2: Streaming Chat
// =============================================================================

Console.WriteLine("=== Streaming Chat ===");

Console.Write("AI Response: ");

await foreach (var chunk in aiService.Chat.CompleteStreamingAsync("Tell me a short joke"))
{
    Console.Write(chunk.Content);
}
Console.WriteLine("\n");


// =============================================================================
// Example 3: Structured Output
// =============================================================================

Console.WriteLine("=== Structured Output ===");

var carListingText = """
    Check out this stylish 2019 Toyota Camry. It has a clean title, only 40,000 miles 
    on the odometer, and a well-maintained interior. The car offers great fuel efficiency, 
    a spacious trunk, and modern safety features like lane departure alert. 
    Minimum offer price: $18,000. Contact Metro Auto at (555) 111-2222 to schedule a test drive.
    """;
var structuredPrompt = $"""
    Convert the following car listing into a JSON object matching this C# schema:
    Condition: "New" or "Used"
    Make: (car manufacturer)
    Model: (car model)
    Year: (four-digit year)
    ListingType: "Sale" or "Lease"
    Price: integer only
    Features: array of short strings
    TenWordSummary: exactly ten words to summarize this listing

    Here is the listing:
    {carListingText}
    """;

var carDetails = await aiService.Chat.CompleteStructuredAsync<CarDetails>(structuredPrompt);
Console.WriteLine($"Parsed Car: {carDetails.Year} {carDetails.Make} {carDetails.Model} - ${carDetails.Price}");
Console.WriteLine($"Summary: {carDetails.TenWordSummary}");
Console.WriteLine($"Fet:{string.Join(", ", carDetails.Features)}");
Console.WriteLine();


// =============================================================================
// Example 4: Function Calling
// =============================================================================
Console.WriteLine("=== Function Calling ===");
var weatherFunction = AIFunctionFactory.Create((string location, string unit) =>
{
    // Simulate weather API call
    var temperature = Random.Shared.Next(5, 20);
    var conditions = Random.Shared.Next(0, 1) == 0 ? "sunny" : "rainy";
    return $"The weather in {location} is {temperature} degrees {unit} and {conditions}.";
}, "get_current_weather", "Get the current weather in a given location");


var chatHistory = new List<ChatMessage>
{
    new(ChatRole.System, "You are a helpful weather assistant."),
    new(ChatRole.User, "What's the weather like in Istanbul? I'm planning a hike.")
};

var functionResponse = await aiService.Chat.CompleteWithFunctionsAsync(chatHistory, [weatherFunction]);
Console.WriteLine($"Function Response: {functionResponse.Messages.LastOrDefault()?.Text}");
Console.WriteLine();

// =============================================================================
// Example 5: Embeddings and Vector Search
// =============================================================================

Console.WriteLine("=== Embeddings and Vector Search ===");

// Generate embeddings
var catEmbedding = await aiService.Embeddings.GenerateEmbeddingAsync("cat");
var dogEmbedding = await aiService.Embeddings.GenerateEmbeddingAsync("dog");
var kittenEmbedding = await aiService.Embeddings.GenerateEmbeddingAsync("kitten");

// Calculate similarities
var catDogSimilarity = await aiService.Embeddings.CalculateSimilarityAsync(catEmbedding, dogEmbedding);
var catKittenSimilarity = await aiService.Embeddings.CalculateSimilarityAsync(catEmbedding, kittenEmbedding);

Console.WriteLine($"Cat-Dog similarity: {catDogSimilarity:F3}");
Console.WriteLine($"Cat-Kitten similarity: {catKittenSimilarity:F3}");

var movies = new List<Movie>
{
    new() { Key = 1, Title = "Lion King", Description = "A young lion's journey to become king" },
    new() { Key = 2, Title = "Inception", Description = "A sci-fi thriller about entering dreams" },
    new() { Key = 3, Title = "The Matrix", Description = "A hacker discovers reality is a simulation" }
};

var moviesCollection = await aiService.VectorStore.GetCollectionAsync<int, Movie>("movies");

// Add movies with embeddings
foreach (var movie in movies)
{
    movie.Vector = await aiService.Embeddings.GenerateEmbeddingAsync(movie.Description);
    await aiService.VectorStore.UpsertAsync("movies", movie.Key, movie);
}

// Search for movies
var queryEmbedding = await aiService.Embeddings.GenerateEmbeddingAsync("science fiction movie");
var searchResults = await aiService.VectorStore.SearchAsync<Movie>("movies", queryEmbedding, top: 2);

Console.WriteLine("Movie Search Results:");
foreach (var result in searchResults)
    {
        Console.WriteLine($"- {result.Record.Title} (Score: {result.Score:F3})");
    }
Console.WriteLine();


// =============================================================================
// Example 6: Vision Analysis
// =============================================================================
Console.WriteLine("=== Vision Analysis ===");

// Simulate image analysis (you would use real image files)
try
{
    // Basic image analysis
    var imageAnalysisPrompt = "Describe what you see in this image";
    // var visionResponse = await aiService.Vision.AnalyzeImageAsync("path/to/image.jpg", imageAnalysisPrompt);
    // Console.WriteLine($"Image Analysis: {visionResponse.Content}");

    // Structured image analysis
    var trafficPrompt = """
        Extract information from this traffic camera image.
        Respond with JSON: { "Status": "Clear/Flowing/Congested/Blocked", "NumCars": number, "NumTrucks": number }
        """;

    // var trafficResult = await aiService.Vision.AnalyzeImageStructuredAsync<TrafficCamResult>("path/to/traffic.jpg", trafficPrompt);
    // Console.WriteLine($"Traffic Status: {trafficResult.Status} (Cars: {trafficResult.NumCars}, Trucks: {trafficResult.NumTrucks})");

    Console.WriteLine("Vision analysis examples require actual image files.");
} catch (Exception ex)
{
    Console.WriteLine($"Vision analysis not available: {ex.Message}");
}

// =============================================================================
// Example 7: Provider Switching
// =============================================================================
Console.WriteLine("=== Provider Switching ===");

// Use OpenAI
var openAiResponse = await aiService.Chat
    .WithProvider("OpenAI")
    .WithModel("gpt-4")
    .CompleteAsync("Say hello from OpenAI");
Console.WriteLine($"OpenAI: {openAiResponse.Messages.LastOrDefault()?.Text}");

// Use Ollama
try
{
    var ollamaResponse = await aiService.Chat
        .WithProvider("Ollama")
        .WithModel("llama3.2")
        .CompleteAsync("Say hello from Ollama");
    Console.WriteLine($"Ollama: {ollamaResponse.Messages.LastOrDefault()?.Text}");
} catch (Exception ex)
{
    Console.WriteLine($"Ollama not available: {ex.Message}");
}

Console.WriteLine("=== All Examples Completed ===");

// =============================================================================
// Supporting Models
// =============================================================================

public class CarDetails
{
    public required string Condition { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public CarListingType ListingType { get; set; }
    public int Price { get; set; }
    public required string[] Features { get; set; }
    public required string TenWordSummary { get; set; }
}

public enum CarListingType { Sale, Lease }

// Movie class for vector search example

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

public class TrafficCamResult
{
    public TrafficStatus Status { get; set; }
    public int NumCars { get; set; }
    public int NumTrucks { get; set; }

    public enum TrafficStatus { Clear, Flowing, Congested, Blocked }
}

