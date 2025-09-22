namespace AI.Bridge.AIWrapper.Core.Models;

public record VisionServiceOptions
{
    public string MaxImageSize { get; init; } = "5MB";
    public string[] SupportedFormats { get; init; } = ["jpg", "png", "webp"];
}
