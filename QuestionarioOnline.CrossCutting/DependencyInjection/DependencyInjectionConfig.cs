using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuestionarioOnline.Application.Interfaces;
using QuestionarioOnline.Application.Services;
using QuestionarioOnline.Domain.Constants;
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
        string storageConnectionString)
    {
        services.AddDbContext<QuestionarioOnlineDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IQuestionarioRepository, QuestionarioRepository>();
        services.AddScoped<IRespostaRepository, RespostaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        services.AddSingleton<IMessageQueue>(sp =>
        {
            var logger = sp.GetService<ILogger<AzureQueueStorageAdapter>>();
            
            var options = new MessageQueueOptions
            {
                MaxRetryAttempts = 5,
                VisibilityTimeoutSeconds = 30,
                MessageTimeToLiveHours = 168,
                EnableDeadLetterQueue = true,
                DeadLetterQueueSuffix = QueueConstants.DeadLetterSuffix,
                EnableTelemetry = true,
                ExponentialBackoffBaseSeconds = 2.0
            };

            return new AzureQueueStorageAdapter(storageConnectionString, options, logger);
        });

        services.AddSingleton<IQueueHealthMonitor>(sp => 
            new QueueHealthMonitor(storageConnectionString));

        services.AddScoped<IQuestionarioService, QuestionarioService>();
        services.AddScoped<IRespostaService, RespostaService>();
        services.AddScoped<IAuthService, Application.Services.AuthService>();

        services.AddValidatorsFromAssembly(Assembly.Load("QuestionarioOnline.Application"));

        return services;
    }
}
