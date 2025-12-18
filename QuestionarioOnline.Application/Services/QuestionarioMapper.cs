using QuestionarioOnline.Application.DTOs.Responses;
using QuestionarioOnline.Domain.Entities;

namespace QuestionarioOnline.Application.Services;

internal static class QuestionarioMapper
{
    public static QuestionarioDto ToDto(Questionario q) => new(
        q.Id,
        q.Titulo,
        q.Descricao,
        q.Status.ToString(),
        q.PeriodoColeta.DataInicio,
        q.PeriodoColeta.DataFim,
        q.DataCriacao,
        q.DataEncerramento,
        q.Perguntas.Select(ToPerguntaDto).ToList());

    public static QuestionarioPublicoDto ToPublicoDto(Questionario q) => new(
        q.Id,
        q.Titulo,
        q.Descricao,
        q.Perguntas.Select(ToPerguntaDto).ToList());

    public static QuestionarioListaDto ToListaDto(Questionario q) => new(
        q.Id,
        q.Titulo,
        q.Status.ToString(),
        q.PeriodoColeta.DataInicio,
        q.PeriodoColeta.DataFim,
        q.Perguntas.Count);

    private static PerguntaDto ToPerguntaDto(Pergunta p) => new(
        p.Id,
        p.Texto,
        p.Ordem,
        p.Obrigatoria,
        p.Opcoes.Select(o => new OpcaoDto(o.Id, o.Texto, o.Ordem)).ToList());
}
