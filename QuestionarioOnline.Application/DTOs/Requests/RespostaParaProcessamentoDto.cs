namespace QuestionarioOnline.Application.DTOs.Requests;

/// <summary>
/// DTO para processamento assíncrono de respostas via fila
/// API já validou completude antes de enfileirar
/// </summary>
public record RespostaParaProcessamentoDto(
    Guid QuestionarioId,
    List<RespostaItemDto> Respostas,
    string IpAddress,
    string UserAgent,
    string? Estado,
    string? Cidade,
    string? RegiaoGeografica
);
