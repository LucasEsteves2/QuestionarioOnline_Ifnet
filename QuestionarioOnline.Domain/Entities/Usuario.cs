using QuestionarioOnline.Domain.Enums;
using QuestionarioOnline.Domain.Exceptions;
using QuestionarioOnline.Domain.ValueObjects;

namespace QuestionarioOnline.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public Email Email { get; private set; }
    public string SenhaHash { get; private set; }
    public UsuarioRole Role { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public bool Ativo { get; private set; }

    private Usuario() { }

    public Usuario(string nome, Email email, string senhaHash, UsuarioRole role = UsuarioRole.Analista)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome, nameof(nome));
        ArgumentNullException.ThrowIfNull(email, nameof(email));
        ArgumentException.ThrowIfNullOrWhiteSpace(senhaHash, nameof(senhaHash));

        Id = Guid.NewGuid();
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        Role = role;
        DataCriacao = DateTime.UtcNow;
        Ativo = true;
    }

    public void GarantirQueEstaAtivo()
    {
        if (!Ativo)
            throw new DomainException("Usuário está inativo e não pode realizar esta operação");
    }

    public void Desativar()
    {
        Ativo = false;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void AtualizarNome(string nome)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome, nameof(nome));
        Nome = nome;
    }

    public void AtualizarSenha(string novaSenhaHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(novaSenhaHash, nameof(novaSenhaHash));
        SenhaHash = novaSenhaHash;
    }

    public void AlterarRole(UsuarioRole novaRole)
    {
        Role = novaRole;
    }
}
