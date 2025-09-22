using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Bridge.AIWrapper.Core.Models;

public record EmbeddingServiceOptions
{
    public int DefaultDimensions { get; init; } = 384;
    public int BatchSize { get; init; } = 100;
}
