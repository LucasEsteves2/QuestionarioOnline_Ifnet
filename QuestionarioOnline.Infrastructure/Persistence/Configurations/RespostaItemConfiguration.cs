using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestionarioOnline.Domain.Entities;

namespace QuestionarioOnline.Infrastructure.Persistence.Configurations;

public class RespostaItemConfiguration : IEntityTypeConfiguration<RespostaItem>
{
    public void Configure(EntityTypeBuilder<RespostaItem> builder)
    {
        builder.ToTable("RespostaItens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RespostaId)
            .IsRequired();

        builder.Property(e => e.PerguntaId)
            .IsRequired();

        builder.Property(e => e.OpcaoRespostaId)
            .IsRequired();

        // Use Restrict to avoid SQL Server multiple cascade paths when deleting Questionario
        builder.HasOne<Pergunta>()
            .WithMany()
            .HasForeignKey(e => e.PerguntaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<OpcaoResposta>()
            .WithMany()
            .HasForeignKey(e => e.OpcaoRespostaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.RespostaId, e.PerguntaId })
            .IsUnique()
            .HasDatabaseName("IX_RespostaItens_RespostaId_PerguntaId");

        builder.HasIndex(e => e.OpcaoRespostaId)
            .HasDatabaseName("IX_RespostaItens_OpcaoRespostaId");
    }
}
