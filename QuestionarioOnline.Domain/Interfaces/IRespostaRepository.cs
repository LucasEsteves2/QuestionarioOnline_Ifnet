using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.ValueObjects;

namespace QuestionarioOnline.Domain.Interfaces;

public interface IRespostaRepository
{
    Task<Resposta?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Resposta>> ObterPorQuestionarioAsync(Guid questionarioId, CancellationToken cancellationToken = default);
    Task<bool> JaRespondeuAsync(Guid questionarioId, OrigemResposta origemResposta, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Resposta resposta, CancellationToken cancellationToken = default);
    Task<int> ContarRespostasPorQuestionarioAsync(Guid questionarioId, CancellationToken cancellationToken = default);
    Task DeletarPorQuestionarioAsync(Guid questionarioId, CancellationToken cancellationToken = default);
}
