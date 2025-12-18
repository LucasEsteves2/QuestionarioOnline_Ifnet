using QuestionarioOnline.Application.DTOs.Responses;

namespace QuestionarioOnline.Api.Contracts.Responses;

public record ResultadoPerguntaResponse(
    Guid Id,
    string Texto,
    List<ResultadoOpcaoResponse> Opcoes
)
{
    public static ResultadoPerguntaResponse From(ResultadoPerguntaDto dto) => new(
        dto.PerguntaId,
        dto.Texto,
        dto.Opcoes.Select(ResultadoOpcaoResponse.From).ToList()
    );
}
