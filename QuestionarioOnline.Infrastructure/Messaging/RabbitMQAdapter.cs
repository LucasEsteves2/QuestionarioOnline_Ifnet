using Microsoft.Extensions.Logging;
using QuestionarioOnline.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace QuestionarioOnline.Infrastructure.Messaging;

/// <summary>
/// Implementação do IMessageQueue usando RabbitMQ
/// </summary>
public class RabbitMQAdapter : IMessageQueue, IAsyncDisposable
{
    private readonly RabbitMQOptions _options;
    private readonly ILogger<RabbitMQAdapter>? _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly object _lock = new();
    private bool _disposed;

    public RabbitMQAdapter(RabbitMQOptions options, ILogger<RabbitMQAdapter>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        
        InitializeConnectionAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeConnectionAsync()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(_options.RetryDelaySeconds)
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Criar exchange principal
            await _channel.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: _options.ExchangeType,
                durable: _options.DurableExchange,
                autoDelete: false,
                arguments: null
            );

            // Criar Dead Letter Exchange se habilitado
            if (_options.EnableDeadLetterExchange)
            {
                await _channel.ExchangeDeclareAsync(
                    exchange: _options.DeadLetterExchangeName,
                    type: "direct",
                    durable: true,
                    autoDelete: false,
                    arguments: null
                );
            }

            _logger?.LogInformation(
                "Conexão RabbitMQ estabelecida com sucesso - Host: {Host}:{Port}, VHost: {VHost}",
                _options.HostName, _options.Port, _options.VirtualHost
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao conectar ao RabbitMQ - Host: {Host}:{Port}", 
                _options.HostName, _options.Port);
            throw;
        }
    }

    public async Task SendAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Nome da fila não pode ser vazio", nameof(queueName));

        if (message is null)
            throw new ArgumentNullException(nameof(message));

        await EnsureConnectionIsOpenAsync();
        await EnsureQueueExistsAsync(queueName);

        var body = SerializeMessage(message);
        var properties = CreateMessageProperties();

        try
        {
            await _channel!.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken
            );

            _logger?.LogDebug(
                "Mensagem enviada com sucesso para a fila '{QueueName}' - Tipo: {MessageType}",
                queueName, typeof(T).Name
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, 
                "Erro ao enviar mensagem para a fila '{QueueName}' - Tipo: {MessageType}",
                queueName, typeof(T).Name
            );
            throw;
        }
    }

    private async Task EnsureConnectionIsOpenAsync()
    {
        if (_connection == null || !_connection.IsOpen || _channel == null || !_channel.IsOpen)
        {
            _logger?.LogWarning("Reconectando ao RabbitMQ...");
            await DisposeAsync();
            await InitializeConnectionAsync();
        }
    }

    private async Task EnsureQueueExistsAsync(string queueName)
    {
        var arguments = new Dictionary<string, object?>();

        // Configurar Dead Letter Exchange
        if (_options.EnableDeadLetterExchange)
        {
            arguments.Add("x-dead-letter-exchange", _options.DeadLetterExchangeName);
            arguments.Add("x-dead-letter-routing-key", $"{queueName}{_options.DeadLetterQueueSuffix}");
        }

        // Configurar TTL se especificado
        if (_options.MessageTTLMilliseconds > 0)
        {
            arguments.Add("x-message-ttl", _options.MessageTTLMilliseconds);
        }

        // Declarar fila principal
        await _channel!.QueueDeclareAsync(
            queue: queueName,
            durable: _options.DurableQueues,
            exclusive: false,
            autoDelete: false,
            arguments: arguments
        );

        // Fazer bind da fila com o exchange
        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: _options.ExchangeName,
            routingKey: queueName
        );

        // Criar Dead Letter Queue se habilitado
        if (_options.EnableDeadLetterExchange)
        {
            var dlqName = $"{queueName}{_options.DeadLetterQueueSuffix}";
            
            await _channel.QueueDeclareAsync(
                queue: dlqName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            await _channel.QueueBindAsync(
                queue: dlqName,
                exchange: _options.DeadLetterExchangeName,
                routingKey: dlqName
            );
        }
    }

    private byte[] SerializeMessage<T>(T message)
    {
        var json = JsonSerializer.Serialize(message);
        return Encoding.UTF8.GetBytes(json);
    }

    private BasicProperties CreateMessageProperties()
    {
        var properties = new BasicProperties
        {
            Persistent = _options.DurableMessages,
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            DeliveryMode = _options.DurableMessages ? DeliveryModes.Persistent : DeliveryModes.Transient,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };
        
        return properties;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        try
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }

            _logger?.LogInformation("Conexão RabbitMQ encerrada com sucesso");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao fechar conexão RabbitMQ");
        }
        finally
        {
            _disposed = true;
        }
    }
}
