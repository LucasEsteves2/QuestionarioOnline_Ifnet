using QuestionarioOnline.Domain.Enums;

namespace QuestionarioOnline.Api.Authorization;

public static class Roles
{
    public const string Admin = nameof(UsuarioRole.Admin);
    public const string Analista = nameof(UsuarioRole.Analista);
    public const string Visualizador = nameof(UsuarioRole.Visualizador);
}
