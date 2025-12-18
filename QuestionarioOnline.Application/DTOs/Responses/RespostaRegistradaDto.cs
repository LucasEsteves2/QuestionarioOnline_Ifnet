namespace QuestionarioOnline.Application.DTOs.Responses;

/// <summary>
/// DTO de confirmação de resposta registrada
/// </summary>
public record RespostaRegistradaDto(
    Guid Id,
    Guid QuestionarioId,
    DateTime DataResposta
);
