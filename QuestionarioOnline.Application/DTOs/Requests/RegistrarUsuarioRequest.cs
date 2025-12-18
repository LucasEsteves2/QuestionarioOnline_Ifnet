namespace QuestionarioOnline.Application.DTOs.Requests;

public record RegistrarUsuarioRequest(
    string Nome,
    string Email,
    string Senha
);
