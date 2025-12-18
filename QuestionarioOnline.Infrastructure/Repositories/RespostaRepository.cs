using Microsoft.EntityFrameworkCore;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Interfaces;
using QuestionarioOnline.Domain.ValueObjects;
using QuestionarioOnline.Infrastructure.Persistence;

namespace QuestionarioOnline.Infrastructure.Repositories;

public class RespostaRepository : IRespostaRepository
{
    private readonly QuestionarioOnlineDbContext _context;

    public RespostaRepository(QuestionarioOnlineDbContext context)
    {
        _context = context;
    }

    public async Task<Resposta?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Respostas
            .Include(r => r.Itens)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Resposta>> ObterPorQuestionarioAsync(Guid questionarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Respostas
            .Include(r => r.Itens)
            .Where(r => r.QuestionarioId == questionarioId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> JaRespondeuAsync(Guid questionarioId, OrigemResposta origemResposta, CancellationToken cancellationToken = default)
    {
        return await _context.Respostas
            .AnyAsync(r => r.QuestionarioId == questionarioId && 
                          r.OrigemResposta.Hash == origemResposta.Hash, 
                     cancellationToken);
    }

    public async Task AdicionarAsync(Resposta resposta, CancellationToken cancellationToken = default)
    {
        await _context.Respostas.AddAsync(resposta, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ContarRespostasPorQuestionarioAsync(Guid questionarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Respostas
            .CountAsync(r => r.QuestionarioId == questionarioId, cancellationToken);
    }
}
