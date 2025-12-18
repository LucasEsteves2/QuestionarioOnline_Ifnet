using QuestionarioOnline.Application.DTOs.Responses;

namespace QuestionarioOnline.Api.Contracts.Responses;

public record RespostaRegistradaResponse(
    Guid Id,
    Guid QuestionarioId,
    DateTime DataRegistro
)
{
    public static RespostaRegistradaResponse From(RespostaRegistradaDto dto) => new(
        dto.Id,
        dto.QuestionarioId,
        dto.DataResposta
    );
}
