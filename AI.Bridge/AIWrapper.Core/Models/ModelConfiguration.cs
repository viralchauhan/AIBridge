using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Bridge.AIWrapper.Core.Models;

public record ModelConfiguration
{
    public string? Chat { get; init; }
    public string? Embeddings { get; init; }
    public string? Vision { get; init; }
}
