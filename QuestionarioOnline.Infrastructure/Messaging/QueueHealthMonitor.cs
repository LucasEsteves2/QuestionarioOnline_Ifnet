using Azure.Storage.Queues;
using QuestionarioOnline.Domain.Constants;

namespace QuestionarioOnline.Infrastructure.Messaging;

/// <summary>
/// Utilitário para monitorar saúde e métricas das filas
/// </summary>
public class QueueHealthMonitor : IQueueHealthMonitor
{
    private readonly string _connectionString;

    public QueueHealthMonitor(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    /// <summary>
    /// Obtém métricas de uma fila
    /// </summary>
    public async Task<QueueMetrics> GetMetricsAsync(string queueName, CancellationToken cancellationToken = default)
    {
        var queueClient = new QueueClient(_connectionString, queueName);
        
        if (!await queueClient.ExistsAsync(cancellationToken))
        {
            return new QueueMetrics
            {
                QueueName = queueName,
                Exists = false
            };
        }

        var properties = await queueClient.GetPropertiesAsync(cancellationToken);

        return new QueueMetrics
        {
            QueueName = queueName,
            Exists = true,
            ApproximateMessagesCount = properties.Value.ApproximateMessagesCount,
            Metadata = properties.Value.Metadata
        };
    }

    /// <summary>
    /// Obtém métricas de Dead Letter Queue
    /// </summary>
    public async Task<QueueMetrics> GetDeadLetterMetricsAsync(
        string queueName, 
        string? deadLetterSuffix = null, 
        CancellationToken cancellationToken = default)
    {
        var suffix = deadLetterSuffix ?? QueueConstants.DeadLetterSuffix;
        var deadLetterQueueName = $"{queueName}{suffix}";
        return await GetMetricsAsync(deadLetterQueueName, cancellationToken);
    }

    /// <summary>
    /// Verifica se a fila está saudável (poucos erros, sem sobrecarga)
    /// </summary>
    public async Task<HealthStatus> CheckHealthAsync(string queueName, CancellationToken cancellationToken = default)
    {
        var metrics = await GetMetricsAsync(queueName, cancellationToken);
        var deadLetterMetrics = await GetDeadLetterMetricsAsync(queueName, cancellationToken: cancellationToken);

        if (!metrics.Exists)
        {
            return CreateUnhealthyStatus("Fila não existe", metrics);
        }

        return EvaluateHealthStatus(metrics, deadLetterMetrics);
    }

    private static HealthStatus CreateUnhealthyStatus(string message, QueueMetrics metrics)
    {
        return new HealthStatus
        {
            IsHealthy = false,
            Status = "Unhealthy",
            Message = message,
            QueueMetrics = metrics
        };
    }

    private static HealthStatus EvaluateHealthStatus(QueueMetrics metrics, QueueMetrics deadLetterMetrics)
    {
        var isHealthy = true;
        var warnings = new List<string>();

        // Verificar Dead Letter Queue
        if (deadLetterMetrics.Exists && 
            deadLetterMetrics.ApproximateMessagesCount > QueueConstants.DeadLetterWarningThreshold)
        {
            warnings.Add($"Dead Letter Queue com {deadLetterMetrics.ApproximateMessagesCount} mensagens (> {QueueConstants.DeadLetterWarningThreshold})");
            isHealthy = false;
        }

        // Verificar backlog
        if (metrics.ApproximateMessagesCount > QueueConstants.BacklogCriticalThreshold)
        {
            warnings.Add($"Backlog CRÍTICO: {metrics.ApproximateMessagesCount} mensagens (> {QueueConstants.BacklogCriticalThreshold:N0})");
            isHealthy = false;
        }
        else if (metrics.ApproximateMessagesCount > QueueConstants.BacklogWarningThreshold)
        {
            warnings.Add($"Backlog alto: {metrics.ApproximateMessagesCount} mensagens (> {QueueConstants.BacklogWarningThreshold:N0})");
        }

        return new HealthStatus
        {
            IsHealthy = isHealthy,
            Status = isHealthy ? "Healthy" : "Degraded",
            Message = isHealthy ? "Fila operando normalmente" : string.Join("; ", warnings),
            QueueMetrics = metrics,
            DeadLetterQueueMetrics = deadLetterMetrics,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Reprocessa mensagens da Dead Letter Queue
    /// </summary>
    public async Task<ReprocessResult> ReprocessDeadLetterMessagesAsync(
        string queueName, 
        int maxMessages = 10, 
        CancellationToken cancellationToken = default)
    {
        var deadLetterQueueName = $"{queueName}{QueueConstants.DeadLetterSuffix}";
        var deadLetterClient = new QueueClient(_connectionString, deadLetterQueueName);
        var originalQueueClient = new QueueClient(_connectionString, queueName);

        if (!await deadLetterClient.ExistsAsync(cancellationToken))
        {
            return new ReprocessResult { ReprocessedCount = 0, FailedCount = 0 };
        }

        var messages = await deadLetterClient.ReceiveMessagesAsync(maxMessages, cancellationToken: cancellationToken);
        
        int reprocessed = 0;
        int failed = 0;

        foreach (var message in messages.Value)
        {
            try
            {
                await originalQueueClient.SendMessageAsync(message.MessageText, cancellationToken: cancellationToken);
                await deadLetterClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);
                reprocessed++;
            }
            catch
            {
                failed++;
            }
        }

        return new ReprocessResult 
        { 
            ReprocessedCount = reprocessed, 
            FailedCount = failed 
        };
    }
}

public class QueueMetrics
{
    public string QueueName { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public int ApproximateMessagesCount { get; set; }
    public IDictionary<string, string>? Metadata { get; set; }
}

public class HealthStatus
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public QueueMetrics? QueueMetrics { get; set; }
    public QueueMetrics? DeadLetterQueueMetrics { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class ReprocessResult
{
    public int ReprocessedCount { get; set; }
    public int FailedCount { get; set; }
    public int TotalProcessed => ReprocessedCount + FailedCount;
    public bool HasFailures => FailedCount > 0;
}
