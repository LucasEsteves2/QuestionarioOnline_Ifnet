using QuestionarioOnline.Application.DTOs.Requests;
using QuestionarioOnline.Application.DTOs.Responses;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.ValueObjects;

namespace QuestionarioOnline.Application.Interfaces;

public interface IRespostaService
{
    /// <summary>
    /// Registra resposta enfileirando para processamento assíncrono (usado pela API)
    /// </summary>
    Task<Result<RespostaRegistradaDto>> RegistrarRespostaAsync(RegistrarRespostaRequestDto request);

    /// <summary>
    /// Processa resposta salvando no banco de dados (usado pelo Worker)
    /// Assume que a API já validou o questionário antes de enfileirar
    /// </summary>
    Task<Result<Resposta>> ProcessarRespostaAsync(
        RespostaParaProcessamentoDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém as respostas de um questionário
    /// </summary>
    Task<Result<IEnumerable<RespostaDto>>> ObterRespostasPorQuestionarioAsync(
        Guid questionarioId,
        CancellationToken cancellationToken = default);
}
