using QuestionarioOnline.Application.DTOs.Requests;
using QuestionarioOnline.Application.DTOs.Responses;
using QuestionarioOnline.Application.Interfaces;
using QuestionarioOnline.Application.Validators;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Interfaces;
using QuestionarioOnline.Domain.ValueObjects;

namespace QuestionarioOnline.Application.Services;

public class QuestionarioService : IQuestionarioService
{
    private readonly IQuestionarioRepository _questionarioRepository;
    private readonly IRespostaRepository _respostaRepository;
    private readonly CriarQuestionarioRequestValidator _validator;

    public QuestionarioService(IQuestionarioRepository questionarioRepository, IRespostaRepository respostaRepository, CriarQuestionarioRequestValidator validator)
    {
        _questionarioRepository = questionarioRepository;
        _respostaRepository = respostaRepository;
        _validator = validator;
    }

    public async Task<Result<QuestionarioDto>> CriarQuestionarioAsync(CriarQuestionarioRequest request, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<QuestionarioDto>($"Erro de validação: {errors}");
        }

        var questionario = Questionario.Criar(request.Titulo, request.Descricao, request.DataInicio, request.DataFim, usuarioId);

        foreach (var perguntaDto in request.Perguntas.OrderBy(p => p.Ordem))
        {
            questionario.AdicionarPergunta(
                perguntaDto.Texto,
                perguntaDto.Ordem,
                perguntaDto.Obrigatoria,
                perguntaDto.Opcoes.Select(o => (o.Texto, o.Ordem))
            );
        }

        await _questionarioRepository.AdicionarAsync(questionario, cancellationToken);

        return Result.Success(MapearParaDto(questionario));
    }

    public async Task<Result<QuestionarioDto>> EncerrarQuestionarioAsync(Guid questionarioId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var questionario = await _questionarioRepository.ObterPorIdComPerguntasAsync(questionarioId, cancellationToken);

        if (questionario is null)
            return Result.Failure<QuestionarioDto>("Questionário não encontrado");

        if (questionario.UsuarioId != usuarioId)
            return Result.Failure<QuestionarioDto>("Usuário não autorizado a encerrar este questionário");

        var resultEncerrar = questionario.Encerrar();

        if (resultEncerrar.IsFailure)
            return Result.Failure<QuestionarioDto>(resultEncerrar.Error);

        await _questionarioRepository.AtualizarAsync(questionario, cancellationToken);

        return Result.Success(MapearParaDto(questionario));
    }

    public async Task<Result> DeletarQuestionarioAsync(Guid questionarioId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var questionario = await _questionarioRepository.ObterPorIdComPerguntasAsync(questionarioId, cancellationToken);

        if (questionario is null)
            return Result.Failure("Questionário não encontrado");

        if (questionario.UsuarioId != usuarioId)
            return Result.Failure("Usuário não autorizado a deletar este questionário");

        var totalRespostas = await _respostaRepository.ContarRespostasPorQuestionarioAsync(questionarioId, cancellationToken);

        if (totalRespostas > 0)
            return Result.Failure("Não é possível deletar questionário que já possui respostas");

        await _questionarioRepository.DeletarAsync(questionario, cancellationToken);

        return Result.Success();
    }


    public async Task<QuestionarioDto?> ObterQuestionarioPorIdAsync(Guid questionarioId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var questionario = await _questionarioRepository.ObterPorIdComPerguntasAsync(questionarioId, cancellationToken);

        if (questionario is null)
            return null;

        if (questionario.UsuarioId != usuarioId)
            return null;

        return MapearParaDto(questionario);
    }

    public async Task<IEnumerable<QuestionarioListaDto>> ListarQuestionariosPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var questionarios = await _questionarioRepository.ObterTodosPorUsuarioAsync(usuarioId, cancellationToken);

        return questionarios.Select(q => new QuestionarioListaDto(
            q.Id,
            q.Titulo,
            q.Status.ToString(),
            q.PeriodoColeta.DataInicio,
            q.PeriodoColeta.DataFim,
            q.Perguntas.Count
        ));
    }

    public async Task<Result<ResultadoQuestionarioDto>> ObterResultadosAsync(Guid questionarioId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var questionario = await _questionarioRepository.ObterPorIdComPerguntasAsync(questionarioId, cancellationToken);

        if (questionario is null)
            return Result.Failure<ResultadoQuestionarioDto>("Questionário não encontrado");

        if (questionario.UsuarioId != usuarioId)
            return Result.Failure<ResultadoQuestionarioDto>("Usuário não autorizado a ver resultados deste questionário");

        var respostas = await _respostaRepository.ObterPorQuestionarioAsync(questionarioId, cancellationToken);
        var totalRespostas = respostas.Count();

        var resultadoPerguntas = questionario.Perguntas.Select(pergunta =>
        {
            var resultadoOpcoes = pergunta.Opcoes.Select(opcao =>
            {
                var totalVotos = respostas.SelectMany(r => r.Itens)
                    .Count(item => item.OpcaoRespostaId == opcao.Id);

                var percentual = totalRespostas > 0
                    ? (totalVotos * 100.0) / totalRespostas
                    : 0;

                return new ResultadoOpcaoDto(opcao.Id, opcao.Texto, totalVotos, percentual);

            }).ToList();

            return new ResultadoPerguntaDto(pergunta.Id, pergunta.Texto, resultadoOpcoes);

        }).ToList();

        var resultado = new ResultadoQuestionarioDto(
            questionario.Id,
            questionario.Titulo,
            totalRespostas,
            resultadoPerguntas
        );

        return Result.Success(resultado);
    }

    private static QuestionarioDto MapearParaDto(Questionario questionario)
    {
        return new QuestionarioDto(
            questionario.Id,
            questionario.Titulo,
            questionario.Descricao,
            questionario.Status.ToString(),
            questionario.PeriodoColeta.DataInicio,
            questionario.PeriodoColeta.DataFim,
            questionario.DataCriacao,
            questionario.DataEncerramento,
            [.. questionario.Perguntas.Select(p => new PerguntaDto(
                p.Id,
                p.Texto,
                p.Ordem,
                p.Obrigatoria,
                p.Opcoes.Select(o => new OpcaoDto(o.Id, o.Texto, o.Ordem)).ToList()
            ))]
        );
    }
}
