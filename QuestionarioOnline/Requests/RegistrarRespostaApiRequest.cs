namespace QuestionarioOnline.Api.Requests;

/// <summary>
/// Request específico da API para registrar resposta.
/// Contém APENAS o que o cliente envia (frontend não conhece IP, UserAgent, etc.)
/// </summary>
public record RegistrarRespostaApiRequest(
    Guid QuestionarioId,
    List<RespostaItemApiDto> Respostas,
    string? Estado = null,
    string? Cidade = null,
    string? RegiaoGeografica = null
);

/// <summary>
/// Item de resposta (API)
/// </summary>
public record RespostaItemApiDto(
    Guid PerguntaId,
    Guid OpcaoRespostaId
);
