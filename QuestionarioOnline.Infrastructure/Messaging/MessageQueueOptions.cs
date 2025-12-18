namespace QuestionarioOnline.Infrastructure.Messaging;

/// <summary>
/// Configurações para fila de mensagens com suporte a retry, dead letter e circuit breaker
/// </summary>
public class MessageQueueOptions
{
    /// <summary>
    /// Número máximo de tentativas antes de enviar para dead letter
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 5;

    /// <summary>
    /// Tempo de invisibilidade da mensagem (visibility timeout) em segundos
    /// Padrão: 30 segundos (Azure Queue Storage default)
    /// </summary>
    public int VisibilityTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Tempo de vida da mensagem (TTL) em horas
    /// Padrão: 7 dias (Azure Queue Storage máximo)
    /// </summary>
    public int MessageTimeToLiveHours { get; set; } = 168; // 7 dias

    /// <summary>
    /// Habilitar Dead Letter Queue automática
    /// </summary>
    public bool EnableDeadLetterQueue { get; set; } = true;

    /// <summary>
    /// Nome da fila de Dead Letter (será criada automaticamente)
    /// </summary>
    public string DeadLetterQueueSuffix { get; set; } = "-deadletter";

    /// <summary>
    /// Habilitar telemetria e logs detalhados
    /// </summary>
    public bool EnableTelemetry { get; set; } = true;

    /// <summary>
    /// Delay exponencial base (em segundos) para retry
    /// </summary>
    public double ExponentialBackoffBaseSeconds { get; set; } = 2.0;
}
