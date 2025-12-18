namespace QuestionarioOnline.Application.DTOs.Responses;

public record UsuarioRegistradoDto(
    Guid UsuarioId,
    string Nome,
    string Email
);
