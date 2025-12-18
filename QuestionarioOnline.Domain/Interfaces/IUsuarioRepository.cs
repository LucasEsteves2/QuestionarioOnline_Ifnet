using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.ValueObjects;

namespace QuestionarioOnline.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
