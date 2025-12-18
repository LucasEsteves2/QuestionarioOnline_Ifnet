namespace QuestionarioOnline.Application.DTOs.Requests;

public record LoginRequest(
    string Email,
    string Senha
);
