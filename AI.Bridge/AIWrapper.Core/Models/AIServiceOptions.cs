using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Bridge.AIWrapper.Core.Models;

public record AIServiceOptions
{
    public string DefaultProvider { get; init; } = "OpenAI";
    public Dictionary<string, ProviderConfiguration> Providers { get; init; } = new();
    public ServiceConfiguration Services { get; init; } = new();
}
