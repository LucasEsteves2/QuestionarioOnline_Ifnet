using Microsoft.EntityFrameworkCore;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Interfaces;
using QuestionarioOnline.Domain.ValueObjects;
using QuestionarioOnline.Infrastructure.Persistence;

namespace QuestionarioOnline.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly QuestionarioOnlineDbContext _context;

    public UsuarioRepository(QuestionarioOnlineDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.Address == email.Address, cancellationToken);
    }

    public async Task<bool> ExisteEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Email.Address == email.Address, cancellationToken);
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
