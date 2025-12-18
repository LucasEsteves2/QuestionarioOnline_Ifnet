using QuestionarioOnline.Application.DTOs.Responses;

namespace QuestionarioOnline.Api.Contracts.Responses;

public record PerguntaResponse(
    Guid Id,
    string Texto,
    int Ordem,
    bool Obrigatoria,
    List<OpcaoResponse> Opcoes
)
{
    public static PerguntaResponse From(PerguntaDto dto) => new(
        dto.Id,
        dto.Texto,
        dto.Ordem,
        dto.Obrigatoria,
        dto.Opcoes.Select(OpcaoResponse.From).ToList()
    );
}
