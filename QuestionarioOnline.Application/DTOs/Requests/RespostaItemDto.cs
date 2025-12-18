namespace QuestionarioOnline.Application.DTOs.Requests;

/// <summary>
/// DTO de um item de resposta (pergunta + opção escolhida)
/// </summary>
public record RespostaItemDto(
    Guid PerguntaId,
    Guid OpcaoRespostaId
);
