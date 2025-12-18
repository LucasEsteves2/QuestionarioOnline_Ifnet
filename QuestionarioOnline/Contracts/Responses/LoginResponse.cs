namespace QuestionarioOnline.Api.Contracts.Responses;

public record LoginResponse(
    string Token,
    Guid UsuarioId,
    string Nome,
    string Email
)
{
    public static LoginResponse From(Application.DTOs.Responses.LoginResponse dto) => new(
        dto.Token,
        dto.UsuarioId,
        dto.Nome,
        dto.Email
    );
}
