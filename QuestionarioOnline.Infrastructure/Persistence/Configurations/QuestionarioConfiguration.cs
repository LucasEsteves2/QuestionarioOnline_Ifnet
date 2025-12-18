using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Enums;

namespace QuestionarioOnline.Infrastructure.Persistence.Configurations;

public class QuestionarioConfiguration : IEntityTypeConfiguration<Questionario>
{
    public void Configure(EntityTypeBuilder<Questionario> builder)
    {
        builder.ToTable("Questionarios");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Titulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Descricao)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.UsuarioId)
            .IsRequired();

        builder.Property(e => e.DataCriacao)
            .IsRequired();

        builder.Property(e => e.DataEncerramento);

        builder.OwnsOne(e => e.PeriodoColeta, periodo =>
        {
            periodo.Property(p => p.DataInicio)
                .HasColumnName("DataInicio")
                .IsRequired();

            periodo.Property(p => p.DataFim)
                .HasColumnName("DataFim")
                .IsRequired();
        });

        builder.HasMany(e => e.Perguntas)
            .WithOne()
            .HasForeignKey("QuestionarioId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Questionarios_Status");

        builder.HasIndex(e => e.UsuarioId)
            .HasDatabaseName("IX_Questionarios_UsuarioId");

        builder.HasIndex(e => e.DataCriacao)
            .HasDatabaseName("IX_Questionarios_DataCriacao");
    }
}
