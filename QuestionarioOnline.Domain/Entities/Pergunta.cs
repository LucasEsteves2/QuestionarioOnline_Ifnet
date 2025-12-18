namespace QuestionarioOnline.Domain.Entities;

public class Pergunta
{
    public Guid Id { get; private set; }
    public Guid QuestionarioId { get; private set; }
    public string Texto { get; private set; }
    public int Ordem { get; private set; }
    public bool Obrigatoria { get; private set; }

    private readonly List<OpcaoResposta> _opcoes = new();
    public IReadOnlyCollection<OpcaoResposta> Opcoes => _opcoes.AsReadOnly();

    private Pergunta() { }

    public Pergunta(Guid questionarioId, string texto, int ordem, bool obrigatoria = true)
    {
        if (questionarioId == Guid.Empty)
            throw new ArgumentException("Questionário inválido", nameof(questionarioId));

        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("Texto da pergunta não pode ser vazio", nameof(texto));

        if (ordem < 0)
            throw new ArgumentException("Ordem deve ser maior ou igual a zero", nameof(ordem));

        Id = Guid.NewGuid();
        QuestionarioId = questionarioId;
        Texto = texto;
        Ordem = ordem;
        Obrigatoria = obrigatoria;
    }

    public void AdicionarOpcoes(IEnumerable<(string Texto, int Ordem)> opcoes)
    {
        foreach (var opcao in opcoes.OrderBy(o => o.Ordem))
        {
            AdicionarOpcao(new OpcaoResposta(Id, opcao.Texto, opcao.Ordem));
        }
    }


    public void AdicionarOpcao(OpcaoResposta opcao)
    {
        if (opcao is null)
            throw new ArgumentNullException(nameof(opcao));

        _opcoes.Add(opcao);
    }

    public void RemoverOpcao(Guid opcaoId)
    {
        var opcao = _opcoes.FirstOrDefault(o => o.Id == opcaoId);
        if (opcao is not null)
            _opcoes.Remove(opcao);
    }

    public void AtualizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("Texto da pergunta não pode ser vazio", nameof(texto));

        Texto = texto;
    }

    public void AtualizarOrdem(int ordem)
    {
        if (ordem < 0)
            throw new ArgumentException("Ordem deve ser maior ou igual a zero", nameof(ordem));

        Ordem = ordem;
    }

    public void TornarObrigatoria()
    {
        Obrigatoria = true;
    }

    public void TornarOpcional()
    {
        Obrigatoria = false;
    }
}
