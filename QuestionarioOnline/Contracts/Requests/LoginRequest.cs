using AppDto = QuestionarioOnline.Application.DTOs.Requests;

namespace QuestionarioOnline.Api.Contracts.Requests;

public record LoginRequest(
    string Email,
    string Senha
)
{
    public AppDto.LoginRequest ToApplicationDto() => new(Email, Senha);
}
