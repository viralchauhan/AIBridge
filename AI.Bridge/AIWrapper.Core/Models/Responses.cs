namespace AI.Bridge.AIWrapper.Core.Models;

public record VisionResponse(string Content, Dictionary<string, object>? Metadata = null);

public record StreamingChatResponse(string? Content, bool IsComplete = false);

public record VectorSearchResult<T>(T Record, float Score) where T : class;

