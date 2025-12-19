namespace QuestionarioOnline.Infrastructure.Messaging;

/// <summary>
/// Opções de configuração para RabbitMQ
/// </summary>
public class RabbitMQOptions
{
    /// <summary>
    /// Hostname do RabbitMQ (padrão: localhost)
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Porta do RabbitMQ (padrão: 5672)
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Nome de usuário para autenticação
    /// </summary>
    public string UserName { get; set; } = "admin";

    /// <summary>
    /// Senha para autenticação
    /// </summary>
    public string Password { get; set; } = "admin123";

    /// <summary>
    /// Virtual host do RabbitMQ (padrão: /)
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Nome do exchange principal
    /// </summary>
    public string ExchangeName { get; set; } = "questionario-exchange";

    /// <summary>
    /// Tipo do exchange (direct, topic, fanout, headers)
    /// </summary>
    public string ExchangeType { get; set; } = "direct";

    /// <summary>
    /// Exchange durável (persiste após restart do RabbitMQ)
    /// </summary>
    public bool DurableExchange { get; set; } = true;

    /// <summary>
    /// Mensagens duráveis (persiste em disco)
    /// </summary>
    public bool DurableMessages { get; set; } = true;

    /// <summary>
    /// Filas duráveis (persiste após restart do RabbitMQ)
    /// </summary>
    public bool DurableQueues { get; set; } = true;

    /// <summary>
    /// Timeout de conexão em segundos
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Número máximo de tentativas de reconexão
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 5;

    /// <summary>
    /// Delay entre tentativas de reconexão em segundos
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 3;

    /// <summary>
    /// Habilitar confirmação de publicação (publisher confirms)
    /// </summary>
    public bool EnablePublisherConfirms { get; set; } = true;

    /// <summary>
    /// Prefetch count - número de mensagens não confirmadas por consumer
    /// </summary>
    public ushort PrefetchCount { get; set; } = 1;

    /// <summary>
    /// TTL padrão das mensagens em milissegundos (0 = sem TTL)
    /// </summary>
    public int MessageTTLMilliseconds { get; set; } = 0;

    /// <summary>
    /// Habilitar Dead Letter Exchange
    /// </summary>
    public bool EnableDeadLetterExchange { get; set; } = true;

    /// <summary>
    /// Nome do Dead Letter Exchange
    /// </summary>
    public string DeadLetterExchangeName { get; set; } = "questionario-dlx";

    /// <summary>
    /// Sufixo para filas de Dead Letter
    /// </summary>
    public string DeadLetterQueueSuffix { get; set; } = ".deadletter";
}
