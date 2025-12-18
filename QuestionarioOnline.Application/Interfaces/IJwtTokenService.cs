using QuestionarioOnline.Domain.Entities;

namespace QuestionarioOnline.Application.Interfaces;

public interface IJwtTokenService
{
    string GerarToken(Usuario usuario);
}
