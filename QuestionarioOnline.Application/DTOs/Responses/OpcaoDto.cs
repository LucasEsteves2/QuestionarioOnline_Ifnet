namespace QuestionarioOnline.Application.DTOs.Responses;

/// <summary>
/// DTO de uma opção de resposta
/// </summary>
public record OpcaoDto(
    Guid Id,
    string Texto,
    int Ordem
);
