using AppDto = QuestionarioOnline.Application.DTOs.Requests;

namespace QuestionarioOnline.Api.Contracts.Requests;

public record RegistrarRespostaRequest(
    Guid QuestionarioId,
    List<RespostaItemRequest> Respostas,
    string? Estado = null,
    string? Cidade = null,
    string? RegiaoGeografica = null
)
{
    public AppDto.RegistrarRespostaRequest ToApplicationDto() => new(
        QuestionarioId,
        Respostas.Select(r => new AppDto.RespostaItemDto(r.PerguntaId, r.OpcaoRespostaId)).ToList(),
        Estado,
        Cidade,
        RegiaoGeografica
    );
}

public record RespostaItemRequest(
    Guid PerguntaId,
    Guid OpcaoRespostaId
);
