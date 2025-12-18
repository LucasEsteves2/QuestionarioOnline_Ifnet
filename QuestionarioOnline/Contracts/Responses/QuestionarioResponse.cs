using QuestionarioOnline.Application.DTOs.Responses;

namespace QuestionarioOnline.Api.Contracts.Responses;

public record QuestionarioResponse(
    Guid Id,
    string Titulo,
    string? Descricao,
    string Status,
    DateTime DataInicio,
    DateTime DataFim,
    DateTime DataCriacao,
    DateTime? DataEncerramento,
    List<PerguntaResponse> Perguntas
)
{
    public static QuestionarioResponse From(QuestionarioDto dto) => new(
        dto.Id,
        dto.Titulo,
        dto.Descricao,
        dto.Status,
        dto.DataInicio,
        dto.DataFim,
        dto.DataCriacao,
        dto.DataEncerramento,
        dto.Perguntas.Select(PerguntaResponse.From).ToList()
    );
}
