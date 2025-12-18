using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Enums;

namespace QuestionarioOnline.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.SenhaHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(UsuarioRole.Analista);

        builder.Property(e => e.DataCriacao)
            .IsRequired();

        builder.Property(e => e.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.OwnsOne(e => e.Email, email =>
        {
            email.Property(e => e.Address)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(255);

            email.HasIndex(e => e.Address)
                .IsUnique()
                .HasDatabaseName("IX_Usuarios_Email");
        });

        builder.HasIndex(e => e.Ativo)
            .HasDatabaseName("IX_Usuarios_Ativo");

        builder.HasIndex(e => e.Role)
            .HasDatabaseName("IX_Usuarios_Role");
    }
}
