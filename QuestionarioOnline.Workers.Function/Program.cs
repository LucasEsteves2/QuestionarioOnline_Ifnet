using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestionarioOnline.CrossCutting.DependencyInjection;

var builder = FunctionsApplication.CreateBuilder(args);

// Configurar connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Registrar todos os serviços (Repositories, Services, RabbitMQ, etc.)
builder.Services.AddQuestionarioOnlineServices(connectionString, builder.Configuration);

builder.Build().Run();
