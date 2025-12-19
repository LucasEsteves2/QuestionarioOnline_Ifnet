using QuestionarioOnline.Application.DTOs.Requests;
using QuestionarioOnline.Application.DTOs.Responses;
using QuestionarioOnline.Application.Interfaces;
using QuestionarioOnline.Application.Validators;
using QuestionarioOnline.Domain.Constants;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Exceptions;
using QuestionarioOnline.Domain.Interfaces;
using QuestionarioOnline.Domain.ValueObjects;

namespace QuestionarioOnline.Application.Services;

public class RespostaService : IRespostaService
{
    private readonly IMessageQueue _messageQueue;
    private readonly IQuestionarioRepository _questionarioRepository;
    private readonly IRespostaRepository _respostaRepository;
    private readonly RegistrarRespostaRequestValidator _validator;

    public RespostaService(
        IMessageQueue messageQueue,
        IQuestionarioRepository questionarioRepository,
        IRespostaRepository respostaRepository,
        RegistrarRespostaRequestValidator validator)
    {
        _messageQueue = messageQueue;
        _questionarioRepository = questionarioRepository;
        _respostaRepository = respostaRepository;
        _validator = validator;
    }

    public async Task<Result<RespostaRegistradaDto>> RegistrarRespostaAsync(RegistrarRespostaRequestDto request)
    {
        var validationResult = await _validator.ValidateAsync(new RegistrarRespostaRequest(
            request.QuestionarioId,
            request.Respostas,
            request.Estado,
            request.Cidade,
            request.RegiaoGeografica
        ));

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<RespostaRegistradaDto>($"Erro de validação: {errors}");
        }

        var questionario = await _questionarioRepository.ObterPorIdComPerguntasAsync(request.QuestionarioId);
        if (questionario is null)
            return Result.Failure<RespostaRegistradaDto>("Questionário não encontrado");

        try
        {
            questionario.GarantirQuePodeReceberRespostas();

            var respostaTemp = Resposta.Criar(
                request.QuestionarioId,
                request.IpAddress,
                request.UserAgent,
                request.Estado,
                request.Cidade,
                request.RegiaoGeografica,
                null,
                null
            );

            foreach (var item in request.Respostas)
            {
                var respostaItem = new RespostaItem(respostaTemp.Id, item.PerguntaId, item.OpcaoRespostaId);
                respostaTemp.AdicionarItem(respostaItem);
            }

            respostaTemp.GarantirCompletude(questionario.Perguntas);

            var mensagem = new RespostaParaProcessamentoDto(
                request.QuestionarioId,
                request.Respostas,
                request.IpAddress,
                request.UserAgent,
                request.Estado,
                request.Cidade,
                request.RegiaoGeografica
            );

            await _messageQueue.SendAsync(QueueConstants.RespostasQueueName, mensagem);

            var resposta = new RespostaRegistradaDto(Guid.NewGuid(), request.QuestionarioId, DateTime.UtcNow);
            return Result.Success(resposta);
        }
        catch (DomainException ex)
        {
            return Result.Failure<RespostaRegistradaDto>(ex.Message);
        }
    }

    public async Task<Result<Resposta>> ProcessarRespostaAsync(
        RespostaParaProcessamentoDto respostaDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resposta = Resposta.Criar(
                respostaDto.QuestionarioId,
                respostaDto.IpAddress,
                respostaDto.UserAgent,
                respostaDto.Estado,
                respostaDto.Cidade,
                respostaDto.RegiaoGeografica,
                null,
                null
            );

            foreach (var item in respostaDto.Respostas)
            {
                var respostaItem = new RespostaItem(resposta.Id, item.PerguntaId, item.OpcaoRespostaId);
                resposta.AdicionarItem(respostaItem);
            }

            await _respostaRepository.AdicionarAsync(resposta, cancellationToken);

            return Result.Success(resposta);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Resposta>(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<RespostaDto>>> ObterRespostasPorQuestionarioAsync(
        Guid questionarioId,
        CancellationToken cancellationToken = default)
    {
        var questionario = await _questionarioRepository.ObterPorIdAsync(questionarioId, cancellationToken);
        if (questionario is null)
            return Result.NotFound<IEnumerable<RespostaDto>>("Questionário não encontrado");

        var respostas = await _respostaRepository.ObterPorQuestionarioAsync(questionarioId, cancellationToken);

        var respostasDto = respostas.Select(r => new RespostaDto(
            r.Id,
            r.QuestionarioId,
            r.DataResposta,
            r.Estado,
            r.Cidade,
            r.RegiaoGeografica,
            r.Itens.Select(i => new RespostaItemRespostaDto(i.PerguntaId, i.OpcaoRespostaId)).ToList()
        ));

        return Result.Success(respostasDto);
    }
}
