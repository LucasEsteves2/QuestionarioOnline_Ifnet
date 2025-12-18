using QuestionarioOnline.Domain.Enums;
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
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));

        if (email is null)
            throw new ArgumentNullException(nameof(email));

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("Senha não pode ser vazia", nameof(senhaHash));

        Id = Guid.NewGuid();
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        Role = role;
        DataCriacao = DateTime.UtcNow;
        Ativo = true;
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
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));

        Nome = nome;
    }

    public void AtualizarSenha(string novaSenhaHash)
    {
        if (string.IsNullOrWhiteSpace(novaSenhaHash))
            throw new ArgumentException("Senha não pode ser vazia", nameof(novaSenhaHash));

        SenhaHash = novaSenhaHash;
    }

    public void AlterarRole(UsuarioRole novaRole)
    {
        Role = novaRole;
    }
}
