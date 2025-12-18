namespace QuestionarioOnline.Api.Contracts.Requests;

public record CriarQuestionarioRequest(
    string Titulo,
    string? Descricao,
    DateTime DataInicio,
    DateTime DataFim,
    List<CriarPerguntaRequest> Perguntas
);

public record CriarPerguntaRequest(
    string Texto,
    int Ordem,
    bool Obrigatoria,
    List<CriarOpcaoRequest> Opcoes
);

public record CriarOpcaoRequest(
    string Texto,
    int Ordem
);
