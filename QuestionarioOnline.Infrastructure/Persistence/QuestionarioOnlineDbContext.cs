using Microsoft.EntityFrameworkCore;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Infrastructure.Persistence.Configurations;

namespace QuestionarioOnline.Infrastructure.Persistence;

public class QuestionarioOnlineDbContext : DbContext
{
    public QuestionarioOnlineDbContext(DbContextOptions<QuestionarioOnlineDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Questionario> Questionarios => Set<Questionario>();
    public DbSet<Pergunta> Perguntas => Set<Pergunta>();
    public DbSet<OpcaoResposta> OpcoesResposta => Set<OpcaoResposta>();
    public DbSet<Resposta> Respostas => Set<Resposta>();
    public DbSet<RespostaItem> RespostaItens => Set<RespostaItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new QuestionarioConfiguration());
        modelBuilder.ApplyConfiguration(new PerguntaConfiguration());
        modelBuilder.ApplyConfiguration(new OpcaoRespostaConfiguration());
        modelBuilder.ApplyConfiguration(new RespostaConfiguration());
        modelBuilder.ApplyConfiguration(new RespostaItemConfiguration());
    }
}
