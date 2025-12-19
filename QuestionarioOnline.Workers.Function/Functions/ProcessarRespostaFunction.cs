using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using QuestionarioOnline.Application.DTOs.Requests;
using QuestionarioOnline.Application.Interfaces;
using System.Text.Json;

namespace QuestionarioOnline.Workers.Function.Functions;

/// <summary>
/// Azure Function para processar respostas de questionários via RabbitMQ
/// Assume que a mensagem já foi validada pela API antes de ser enfileirada
/// </summary>
public class ProcessarRespostaFunction
{
    private readonly IRespostaService _respostaService;
    private readonly ILogger<ProcessarRespostaFunction> _logger;

    public ProcessarRespostaFunction(
        IRespostaService respostaService,
        ILogger<ProcessarRespostaFunction> logger)
    {
        _respostaService = respostaService;
        _logger = logger;
    }

    [Function(nameof(ProcessarRespostaFunction))]
    public async Task Run(
        [RabbitMQTrigger("respostas-questionario", ConnectionStringSetting = "RabbitMQConnection")] 
        string message)
    {
        _logger.LogInformation("Mensagem recebida da fila RabbitMQ");

        try
        {
            var dto = JsonSerializer.Deserialize<RespostaParaProcessamentoDto>(message);
            
            var result = await _respostaService.ProcessarRespostaAsync(dto);

            if (result.IsFailure)
            {
                _logger.LogError("Erro ao processar resposta: {Error} | QuestionarioId: {QuestionarioId}", 
                    result.Error, dto.QuestionarioId);
                return;
            }

            _logger.LogInformation("Resposta processada com sucesso | RespostaId: {RespostaId}", result.Value.Id);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON inválido");
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar mensagem");
            throw;
        }
    }
}
