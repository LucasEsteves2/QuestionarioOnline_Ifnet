using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuestionarioOnline.Application.Interfaces;
using QuestionarioOnline.Application.Services;
using QuestionarioOnline.Domain.Interfaces;
using QuestionarioOnline.Infrastructure.Messaging;
using QuestionarioOnline.Infrastructure.Persistence;
using QuestionarioOnline.Infrastructure.Repositories;
using System.Reflection;

namespace QuestionarioOnline.CrossCutting.DependencyInjection;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddQuestionarioOnlineServices(
        this IServiceCollection services, 
        string connectionString,
        IConfiguration configuration)
    {
        services.AddDbContext<QuestionarioOnlineDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IQuestionarioRepository, QuestionarioRepository>();
        services.AddScoped<IRespostaRepository, RespostaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Configurar RabbitMQ
        services.AddSingleton<IMessageQueue>(sp =>
        {
            var logger = sp.GetService<ILogger<RabbitMQAdapter>>();
            
            var options = new RabbitMQOptions
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = configuration["RabbitMQ:UserName"] ?? "admin",
                Password = configuration["RabbitMQ:Password"] ?? "admin123",
                VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/",
                ExchangeName = configuration["RabbitMQ:ExchangeName"] ?? "questionario-exchange",
                ExchangeType = configuration["RabbitMQ:ExchangeType"] ?? "direct",
                DurableExchange = bool.Parse(configuration["RabbitMQ:DurableExchange"] ?? "true"),
                DurableMessages = bool.Parse(configuration["RabbitMQ:DurableMessages"] ?? "true"),
                DurableQueues = bool.Parse(configuration["RabbitMQ:DurableQueues"] ?? "true"),
                ConnectionTimeoutSeconds = int.Parse(configuration["RabbitMQ:ConnectionTimeoutSeconds"] ?? "30"),
                MaxRetryAttempts = int.Parse(configuration["RabbitMQ:MaxRetryAttempts"] ?? "5"),
                RetryDelaySeconds = int.Parse(configuration["RabbitMQ:RetryDelaySeconds"] ?? "3"),
                EnablePublisherConfirms = bool.Parse(configuration["RabbitMQ:EnablePublisherConfirms"] ?? "false"),
                PrefetchCount = ushort.Parse(configuration["RabbitMQ:PrefetchCount"] ?? "1"),
                MessageTTLMilliseconds = int.Parse(configuration["RabbitMQ:MessageTTLMilliseconds"] ?? "0"),
                EnableDeadLetterExchange = bool.Parse(configuration["RabbitMQ:EnableDeadLetterExchange"] ?? "true"),
                DeadLetterExchangeName = configuration["RabbitMQ:DeadLetterExchangeName"] ?? "questionario-dlx",
                DeadLetterQueueSuffix = configuration["RabbitMQ:DeadLetterQueueSuffix"] ?? ".deadletter"
            };

            return new RabbitMQAdapter(options, logger);
        });

        // Services
        services.AddScoped<IQuestionarioService, QuestionarioService>();
        services.AddScoped<IRespostaService, RespostaService>();
        services.AddScoped<IAuthService, Application.Services.AuthService>();

        // Validators
        services.AddValidatorsFromAssembly(Assembly.Load("QuestionarioOnline.Application"));

        return services;
    }
}
