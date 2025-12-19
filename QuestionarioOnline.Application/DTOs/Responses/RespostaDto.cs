namespace QuestionarioOnline.Application.DTOs.Responses;

public record RespostaDto(
    Guid Id,
    Guid QuestionarioId,
    DateTime DataResposta,
    string? Estado,
    string? Cidade,
    string? RegiaoGeografica,
    List<RespostaItemRespostaDto> Itens
);

public record RespostaItemRespostaDto(
    Guid PerguntaId,
    Guid OpcaoRespostaId
);
