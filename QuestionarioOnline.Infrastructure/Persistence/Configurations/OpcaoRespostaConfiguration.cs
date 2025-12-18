using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestionarioOnline.Domain.Entities;

namespace QuestionarioOnline.Infrastructure.Persistence.Configurations;

public class OpcaoRespostaConfiguration : IEntityTypeConfiguration<OpcaoResposta>
{
    public void Configure(EntityTypeBuilder<OpcaoResposta> builder)
    {
        builder.ToTable("OpcoesResposta");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PerguntaId)
            .IsRequired();

        builder.Property(e => e.Texto)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Ordem)
            .IsRequired();

        builder.HasIndex(e => new { e.PerguntaId, e.Ordem })
            .HasDatabaseName("IX_OpcoesResposta_PerguntaId_Ordem");
    }
}
