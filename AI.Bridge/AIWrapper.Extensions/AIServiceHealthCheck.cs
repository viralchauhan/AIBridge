using AI.Bridge.AIWrapper.Core.Abstractions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Diagnostics.HealthChecks;


namespace AI.Bridge.AIWrapper.Extensions;

public class AIServiceHealthCheck : IHealthCheck
{
    private readonly IAIService _aiService;

    public AIServiceHealthCheck(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "ping")
            };

            // Simple health check - try to complete a basic prompt
            var response = await _aiService.Chat.CompleteAsync(messages, cancellationToken);

            return HealthCheckResult.Healthy("AI service is responding");

        } catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("AI service is not responding", ex);
        }
    }
}

