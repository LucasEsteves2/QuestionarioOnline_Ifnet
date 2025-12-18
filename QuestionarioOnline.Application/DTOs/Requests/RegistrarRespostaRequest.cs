namespace QuestionarioOnline.Application.DTOs.Requests;

/// <summary>
/// Request para registrar resposta de questionário
/// </summary>
public record RegistrarRespostaRequest(
    Guid QuestionarioId,
    List<RespostaItemDto> Respostas,
    string? Estado = null,
    string? Cidade = null,
    string? RegiaoGeografica = null
);
