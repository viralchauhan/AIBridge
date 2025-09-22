

namespace AI.Bridge.AIWrapper.Core.Models;

public record ChatServiceOptions
{
    public int DefaultMaxTokens { get; init; } = 4000;
    public float DefaultTemperature { get; init; } = 0.7f;
    public bool EnableStreaming { get; init; } = true;
}
