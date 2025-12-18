namespace QuestionarioOnline.Application.DTOs.Responses;

/// <summary>
/// DTO de questionário para acesso público (sem dados internos)
/// Usado para exibir questionário para usuários que vão responder
/// </summary>
public record QuestionarioPublicoDto(
    Guid Id,
    string Titulo,
    string? Descricao,
    List<PerguntaDto> Perguntas
);
