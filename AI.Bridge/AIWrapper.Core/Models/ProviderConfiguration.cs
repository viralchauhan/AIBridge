using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Bridge.AIWrapper.Core.Models;

public record ProviderConfiguration
{
    public string? Endpoint { get; init; }
    public string? ApiKey { get; init; }
    public ModelConfiguration Models { get; init; } = new();
    public Dictionary<string, string> AdditionalSettings { get; init; } = new();
}
