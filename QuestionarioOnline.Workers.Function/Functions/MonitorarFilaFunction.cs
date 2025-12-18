using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using QuestionarioOnline.Domain.Constants;
using QuestionarioOnline.Infrastructure.Messaging;

namespace QuestionarioOnline.Workers.Function.Functions;

/// <summary>
/// Função agendada para monitorar saúde das filas e reprocessar Dead Letter Queue
/// Executa diariamente às 00:00 UTC
/// </summary>
public class MonitorarFilaFunction
{
    private readonly ILogger<MonitorarFilaFunction> _logger;
    private readonly IQueueHealthMonitor _healthMonitor;

    public MonitorarFilaFunction(
        ILogger<MonitorarFilaFunction> logger,
        IQueueHealthMonitor healthMonitor)
    {
        _logger = logger;
        _healthMonitor = healthMonitor;
    }

    /// <summary>
    /// Timer Trigger: Executa diariamente às 00:00 UTC
    /// Cron expression: "0 0 0 * * *"
    /// - Formato: {second} {minute} {hour} {day} {month} {day-of-week}
    /// - "0 0 0 * * *" = segundo 0, minuto 0, hora 0, todos os dias
    /// 
    /// Outras opções:
    /// - "0 */30 * * * *" = A cada 30 minutos
    /// - "0 0 */6 * * *" = A cada 6 horas
    /// - "0 0 0 * * 0"   = Todo domingo às 00:00
    /// </summary>
    [Function(nameof(MonitorarFilaFunction))]
    public async Task Run(
        [TimerTrigger("0 0 0 * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("?? Iniciando monitoramento de fila - {Time}", DateTime.UtcNow);

        try
        {
            // 1?? Verificar saúde da fila principal
            var health = await _healthMonitor.CheckHealthAsync(
                QueueConstants.RespostasQueueName, 
                CancellationToken.None);

            if (!health.IsHealthy)
            {
                _logger.LogWarning(
                    "?? Fila não está saudável | Status: {Status} | Message: {Message} | Warnings: {Warnings}",
                    health.Status, health.Message, string.Join(", ", health.Warnings));
            }
            else
            {
                _logger.LogInformation("? Fila principal está saudável");
            }

            // 2?? Verificar Dead Letter Queue
            var deadLetterMetrics = await _healthMonitor.GetDeadLetterMetricsAsync(
                QueueConstants.RespostasQueueName);

            if (!deadLetterMetrics.Exists)
            {
                _logger.LogInformation("?? Dead Letter Queue não existe (será criada na primeira falha)");
                return;
            }

            if (deadLetterMetrics.ApproximateMessagesCount == 0)
            {
                _logger.LogInformation("? Dead Letter Queue vazia - nenhuma ação necessária");
                return;
            }

            // 3?? Dead Letter Queue tem mensagens - iniciar reprocessamento
            _logger.LogInformation(
                "?? Dead Letter Queue contém {Count} mensagens. Iniciando reprocessamento automático...",
                deadLetterMetrics.ApproximateMessagesCount);

            // Reprocessar em lotes de 100 (limite do Azure Queue Storage)
            var result = await _healthMonitor.ReprocessDeadLetterMessagesAsync(
                QueueConstants.RespostasQueueName,
                maxMessages: 100,
                CancellationToken.None);

            _logger.LogInformation(
                "? Reprocessamento concluído | Sucesso: {Success} | Falhas: {Failed} | Total: {Total}",
                result.ReprocessedCount, result.FailedCount, result.TotalProcessed);

            // 4?? Verificar se ainda há muitas mensagens na DLQ após reprocessamento
            if (result.HasFailures)
            {
                _logger.LogWarning(
                    "?? {FailedCount} mensagens FALHARAM ao reprocessar. Verifique logs para detalhes.",
                    result.FailedCount);
            }

            // 5?? Verificar métricas atualizadas após reprocessamento
            var deadLetterMetricsAposReprocessamento = await _healthMonitor.GetDeadLetterMetricsAsync(
                QueueConstants.RespostasQueueName);

            if (deadLetterMetricsAposReprocessamento.ApproximateMessagesCount > QueueConstants.DeadLetterWarningThreshold)
            {
                _logger.LogCritical(
                    "?? ALERTA CRÍTICO: Dead Letter Queue ainda contém {Count} mensagens após reprocessamento! " +
                    "Threshold: {Threshold}. Investigação manual necessária.",
                    deadLetterMetricsAposReprocessamento.ApproximateMessagesCount,
                    QueueConstants.DeadLetterWarningThreshold);
                
                // TODO: Enviar alerta (Email, Teams, PagerDuty, etc.)
                // await _alertService.SendAlertAsync($"DLQ crítico: {count} mensagens");
            }

            // 6?? Log de métricas finais
            var mainQueueMetrics = await _healthMonitor.GetMetricsAsync(
                QueueConstants.RespostasQueueName);

            _logger.LogInformation(
                "?? Métricas Finais | Fila Principal: {MainCount} msgs | DLQ: {DLQCount} msgs",
                mainQueueMetrics.ApproximateMessagesCount,
                deadLetterMetricsAposReprocessamento.ApproximateMessagesCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "? Erro crítico ao monitorar fila. Timer: {IsPastDue}", 
                timerInfo.IsPastDue);
            
            // Não fazer throw - deixa próxima execução tentar novamente
            // throw; 
        }
    }
}
