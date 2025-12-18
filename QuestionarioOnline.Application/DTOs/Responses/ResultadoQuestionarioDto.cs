namespace QuestionarioOnline.Application.DTOs.Responses;

/// <summary>
/// DTO com resultados agregados de um questionário
/// </summary>
public record ResultadoQuestionarioDto(
    Guid QuestionarioId,
    string Titulo,
    int TotalRespostas,
    List<ResultadoPerguntaDto> Perguntas
);
