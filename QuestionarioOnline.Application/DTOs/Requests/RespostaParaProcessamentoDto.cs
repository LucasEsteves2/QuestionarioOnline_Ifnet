namespace QuestionarioOnline.Application.DTOs.Requests;

/// <summary>
/// DTO para processamento assíncrono de respostas via fila
/// </summary>
public record RespostaParaProcessamentoDto(
    Guid QuestionarioId,
    List<RespostaItemDto> Respostas,
    string? Estado,
    string? Cidade,
    string? RegiaoGeografica
);
