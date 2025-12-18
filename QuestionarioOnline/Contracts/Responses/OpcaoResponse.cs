using QuestionarioOnline.Application.DTOs.Responses;

namespace QuestionarioOnline.Api.Contracts.Responses;

public record OpcaoResponse(
    Guid Id,
    string Texto,
    int Ordem
)
{
    public static OpcaoResponse From(OpcaoDto dto) => new(
        dto.Id,
        dto.Texto,
        dto.Ordem
    );
}
