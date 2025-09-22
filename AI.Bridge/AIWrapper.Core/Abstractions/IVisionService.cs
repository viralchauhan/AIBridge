using AI.Bridge.AIWrapper.Core.Models;

namespace AI.Bridge.AIWrapper.Core.Abstractions;

public interface IVisionService
{
    Task<VisionResponse> AnalyzeImageAsync(string imagePath, string prompt, CancellationToken cancellationToken = default);
    Task<VisionResponse> AnalyzeImageAsync(byte[] imageData, string prompt, string mimeType = "image/png", CancellationToken cancellationToken = default);
    Task<T> AnalyzeImageStructuredAsync<T>(string imagePath, string prompt, bool jsonStructureResponce, CancellationToken cancellationToken = default) where T : class;
    Task<T> AnalyzeImageStructuredAsync<T>(byte[] imageData, string prompt,bool isJsonStructureResponce, string mimeType = "image/png", CancellationToken cancellationToken = default) where T : class;
    IVisionService WithProvider(string providerName);
    IVisionService WithModel(string modelName);
}
