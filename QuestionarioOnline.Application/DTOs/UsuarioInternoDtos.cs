namespace QuestionarioOnline.Application.DTOs;

// DTOs para Usuário Interno
public record CriarUsuarioInternoRequest(
    string Nome,
    string Email,
    string Senha
);

public record AutenticarUsuarioRequest(
    string Email,
    string Senha
);

public record UsuarioInternoDto(
    Guid Id,
    string Nome,
    string Email,
    DateTime DataCriacao,
    bool Ativo
);

public record AutenticacaoResultDto(
    bool Sucesso,
    string? Token,
    UsuarioInternoDto? Usuario,
    string? Mensagem
);
