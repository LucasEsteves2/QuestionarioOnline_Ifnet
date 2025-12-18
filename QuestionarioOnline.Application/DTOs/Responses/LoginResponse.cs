namespace QuestionarioOnline.Application.DTOs.Responses;

public record LoginResponse(
    string Token,
    Guid UsuarioId,
    string Nome,
    string Email
);
