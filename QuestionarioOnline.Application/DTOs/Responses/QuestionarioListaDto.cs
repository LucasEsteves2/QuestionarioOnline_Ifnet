namespace QuestionarioOnline.Application.DTOs.Responses;


public record QuestionarioListaDto(
    Guid Id,
    string Titulo,
    string Status,
    DateTime DataInicio,
    DateTime DataFim,
    int TotalPerguntas
);
