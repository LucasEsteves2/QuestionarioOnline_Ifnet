using QuestionarioOnline.Application.DTOs.Responses;

namespace QuestionarioOnline.Api.Contracts.Responses;

public record QuestionarioListaResponse(
    Guid Id,
    string Titulo,
    string Status,
    DateTime DataInicio,
    DateTime DataFim,
    int TotalPerguntas
)
{
    public static QuestionarioListaResponse From(QuestionarioListaDto dto) => new(
        dto.Id,
        dto.Titulo,
        dto.Status,
        dto.DataInicio,
        dto.DataFim,
        dto.TotalPerguntas
    );
}
