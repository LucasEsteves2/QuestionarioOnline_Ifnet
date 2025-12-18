using System.Security.Claims;

namespace QuestionarioOnline.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid ObterUsuarioId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? user.FindFirst("sub")?.Value
                       ?? user.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("Token JWT não contém identificador do usuário");

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Identificador do usuário no token é inválido");

        return userId;
    }

    public static string ObterEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value
            ?? user.FindFirst("email")?.Value
            ?? throw new UnauthorizedAccessException("Token JWT não contém email do usuário");
    }

    public static string ObterNome(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value
            ?? user.FindFirst("name")?.Value
            ?? throw new UnauthorizedAccessException("Token JWT não contém nome do usuário");
    }

    public static bool TemRole(this ClaimsPrincipal user, string role)
    {
        return user.IsInRole(role);
    }
}
