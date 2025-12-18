namespace QuestionarioOnline.Application.DTOs.Responses;

/// <summary>
/// DTO com resultado de uma opção de resposta (votos e percentual)
/// </summary>
public record ResultadoOpcaoDto(
    Guid OpcaoId,
    string Texto,
    int TotalVotos,
    double Percentual
);
