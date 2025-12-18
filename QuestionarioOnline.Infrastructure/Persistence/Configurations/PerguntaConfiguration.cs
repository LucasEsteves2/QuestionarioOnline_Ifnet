using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestionarioOnline.Domain.Entities;

namespace QuestionarioOnline.Infrastructure.Persistence.Configurations;

public class PerguntaConfiguration : IEntityTypeConfiguration<Pergunta>
{
    public void Configure(EntityTypeBuilder<Pergunta> builder)
    {
        builder.ToTable("Perguntas");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.QuestionarioId)
            .IsRequired();

        builder.Property(e => e.Texto)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Ordem)
            .IsRequired();

        builder.Property(e => e.Obrigatoria)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasMany(e => e.Opcoes)
            .WithOne()
            .HasForeignKey("PerguntaId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.QuestionarioId, e.Ordem })
            .HasDatabaseName("IX_Perguntas_QuestionarioId_Ordem");
    }
}
