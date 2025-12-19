using QuestionarioOnline.Application.DTOs.Responses;

namespace QuestionarioOnline.Api.Contracts.Responses;

public record RespostaResponse(
    Guid Id,
    Guid QuestionarioId,
    DateTime DataResposta,
    string? Estado,
    string? Cidade,
    string? RegiaoGeografica,
    List<RespostaItemResponse> Itens
)
{
    public static RespostaResponse From(RespostaDto dto) => new(
        dto.Id,
        dto.QuestionarioId,
        dto.DataResposta,
        dto.Estado,
        dto.Cidade,
        dto.RegiaoGeografica,
        dto.Itens.Select(RespostaItemResponse.From).ToList()
    );
}

public record RespostaItemResponse(
    Guid PerguntaId,
    Guid OpcaoRespostaId
)
{
    public static RespostaItemResponse From(RespostaItemRespostaDto dto) => new(
        dto.PerguntaId,
        dto.OpcaoRespostaId
    );
}
