namespace QuestionarioOnline.Application.DTOs.Responses;

/// <summary>
/// DTO simplificado de questionário para listagem
/// </summary>
public record QuestionarioListaDto(
    Guid Id,
    string Titulo,
    string Status,
    DateTime DataInicio,
    DateTime DataFim,
    int TotalPerguntas
);
