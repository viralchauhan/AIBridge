using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Bridge.AIWrapper.Core.Abstractions;

public interface IAIService
{
    IChatService Chat { get; }
    IEmbeddingService Embeddings { get; }
    IVisionService Vision { get; }
    IVectorStoreService VectorStore { get; }
}
