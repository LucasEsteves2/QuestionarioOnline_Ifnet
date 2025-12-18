using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestionarioOnline.Domain.Interfaces;
using QuestionarioOnline.Infrastructure.Messaging;
using QuestionarioOnline.Infrastructure.Persistence;
using QuestionarioOnline.Infrastructure.Repositories;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Configurar DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<QuestionarioOnlineDbContext>(options =>
    options.UseSqlServer(connectionString));

// Registrar Repositories
builder.Services.AddScoped<IQuestionarioRepository, QuestionarioRepository>();
builder.Services.AddScoped<IRespostaRepository, RespostaRepository>();

// ? Registrar QueueHealthMonitor (para MonitorarFilaFunction)
var storageConnectionString = builder.Configuration.GetConnectionString("AzureWebJobsStorage")
    ?? throw new InvalidOperationException("Connection string 'AzureWebJobsStorage' not found.");

builder.Services.AddSingleton<IQueueHealthMonitor>(sp => 
    new QueueHealthMonitor(storageConnectionString));

// Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
