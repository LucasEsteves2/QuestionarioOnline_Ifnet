namespace QuestionarioOnline.Domain.Entities;

public class OpcaoResposta
{
    public Guid Id { get; private set; }
    public Guid PerguntaId { get; private set; }
    public string Texto { get; private set; }
    public int Ordem { get; private set; }

    private OpcaoResposta() { }

    public OpcaoResposta(Guid perguntaId, string texto, int ordem)
    {
        if (perguntaId == Guid.Empty)
            throw new ArgumentException("Pergunta inválida", nameof(perguntaId));

        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("Texto da opção não pode ser vazio", nameof(texto));

        if (ordem < 0)
            throw new ArgumentException("Ordem deve ser maior ou igual a zero", nameof(ordem));

        Id = Guid.NewGuid();
        PerguntaId = perguntaId;
        Texto = texto;
        Ordem = ordem;
    }

    public void AtualizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("Texto da opção não pode ser vazio", nameof(texto));

        Texto = texto;
    }

    public void AtualizarOrdem(int ordem)
    {
        if (ordem < 0)
            throw new ArgumentException("Ordem deve ser maior ou igual a zero", nameof(ordem));

        Ordem = ordem;
    }
}
