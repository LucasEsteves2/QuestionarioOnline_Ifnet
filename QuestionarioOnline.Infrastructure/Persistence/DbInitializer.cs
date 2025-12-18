using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Enums;
using QuestionarioOnline.Domain.ValueObjects;
using QuestionarioOnline.Infrastructure.Persistence;

namespace QuestionarioOnline.Infrastructure.Extensions;

public static class DbInitializer
{
    public static async Task SeedAsync(QuestionarioOnlineDbContext context)
    {
        if (context.Usuarios.Any())
            return;

        var usuarioAdmin = new Usuario(
            "Administrador",
            Email.Create("admin@questionarioonline.com"),
            BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            UsuarioRole.Admin
        );

        var usuarioAnalista = new Usuario(
            "Analista Teste",
            Email.Create("analista@empresa.com"),
            BCrypt.Net.BCrypt.HashPassword("Senha123"),
            UsuarioRole.Analista
        );

        context.Usuarios.AddRange(usuarioAdmin, usuarioAnalista);

        await context.SaveChangesAsync();
    }
}
