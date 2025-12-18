namespace QuestionarioOnline.Infrastructure.Messaging;

/// <summary>
/// Abstração para monitoramento de saúde de filas (Dependency Inversion Principle)
/// </summary>
public interface IQueueHealthMonitor
{
    Task<QueueMetrics> GetMetricsAsync(string queueName, CancellationToken cancellationToken = default);
    Task<QueueMetrics> GetDeadLetterMetricsAsync(string queueName, string? deadLetterSuffix = null, CancellationToken cancellationToken = default);
    Task<HealthStatus> CheckHealthAsync(string queueName, CancellationToken cancellationToken = default);
    Task<ReprocessResult> ReprocessDeadLetterMessagesAsync(string queueName, int maxMessages = 10, CancellationToken cancellationToken = default);
}
