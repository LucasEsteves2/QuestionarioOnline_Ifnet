namespace QuestionarioOnline.Application.DTOs.Responses;

/// <summary>
/// DTO de uma pergunta do questionário
/// </summary>
public record PerguntaDto(
    Guid Id,
    string Texto,
    int Ordem,
    bool Obrigatoria,
    List<OpcaoDto> Opcoes
);
