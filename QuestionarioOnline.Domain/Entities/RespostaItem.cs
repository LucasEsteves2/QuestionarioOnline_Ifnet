namespace QuestionarioOnline.Domain.Entities;

public class RespostaItem
{
    public Guid Id { get; private set; }
    public Guid RespostaId { get; private set; }
    public Guid PerguntaId { get; private set; }
    public Guid OpcaoRespostaId { get; private set; }

    private RespostaItem() { }

    public RespostaItem(Guid respostaId, Guid perguntaId, Guid opcaoRespostaId)
    {
        if (respostaId == Guid.Empty)
            throw new ArgumentException("Resposta inválida", nameof(respostaId));

        if (perguntaId == Guid.Empty)
            throw new ArgumentException("Pergunta inválida", nameof(perguntaId));

        if (opcaoRespostaId == Guid.Empty)
            throw new ArgumentException("Opção de resposta inválida", nameof(opcaoRespostaId));

        Id = Guid.NewGuid();
        RespostaId = respostaId;
        PerguntaId = perguntaId;
        OpcaoRespostaId = opcaoRespostaId;
    }
}
