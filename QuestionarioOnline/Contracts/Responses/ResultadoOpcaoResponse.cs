using QuestionarioOnline.Application.DTOs.Responses;

namespace QuestionarioOnline.Api.Contracts.Responses;

public record ResultadoOpcaoResponse(
    Guid Id,
    string Texto,
    int Votos,
    double Percentual
)
{
    public static ResultadoOpcaoResponse From(ResultadoOpcaoDto dto) => new(
        dto.OpcaoId,
        dto.Texto,
        dto.TotalVotos,
        dto.Percentual
    );
}
