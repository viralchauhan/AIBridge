using AI.Bridge.AIWrapper.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Bridge.AIWrapper.Services;

public class AIService : IAIService
{
    public IChatService Chat { get; }
    public IEmbeddingService Embeddings { get; }
    public IVisionService Vision { get; }
    public IVectorStoreService VectorStore { get; }

    public AIService(
        IChatService chatService,
        IEmbeddingService embeddingService,
        IVisionService visionService,
        IVectorStoreService vectorStoreService)
    {
        Chat = chatService;
        Embeddings = embeddingService;
        Vision = visionService;
        VectorStore = vectorStoreService;
    }
}
