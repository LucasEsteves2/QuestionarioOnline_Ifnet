namespace QuestionarioOnline.Application.DTOs.Responses;

/// <summary>
/// DTO com resultados de uma pergunta
/// </summary>
public record ResultadoPerguntaDto(
    Guid PerguntaId,
    string Texto,
    List<ResultadoOpcaoDto> Opcoes
);
