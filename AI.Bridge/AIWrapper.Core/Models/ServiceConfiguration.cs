using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Bridge.AIWrapper.Core.Models;

public record ServiceConfiguration
{
    public ChatServiceOptions Chat { get; init; } = new();
    public EmbeddingServiceOptions Embeddings { get; init; } = new();
    public VisionServiceOptions Vision { get; init; } = new();
}
