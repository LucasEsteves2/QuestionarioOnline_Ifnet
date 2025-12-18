using FluentValidation;
using QuestionarioOnline.Application.DTOs.Requests;

namespace QuestionarioOnline.Application.Validators;

/// <summary>
/// Validador para criação de questionário
/// NOTA: Mantido para referência, mas atualmente usando Data Annotations
/// </summary>
public class CriarQuestionarioRequestValidator : AbstractValidator<CriarQuestionarioRequest>
{
    public CriarQuestionarioRequestValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("Título é obrigatório")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres");

        RuleFor(x => x.Descricao)
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descricao));

        RuleFor(x => x.DataInicio)
            .NotEmpty().WithMessage("Data de início é obrigatória")
            .LessThan(x => x.DataFim).WithMessage("Data de início deve ser anterior à data de fim");

        RuleFor(x => x.DataFim)
            .NotEmpty().WithMessage("Data de fim é obrigatória")
            .GreaterThan(DateTime.UtcNow).WithMessage("Data de fim deve ser futura");

        RuleFor(x => x.Perguntas)
            .NotEmpty().WithMessage("Questionário deve ter pelo menos uma pergunta")
            .Must(perguntas => perguntas != null && perguntas.Count >= 1)
            .WithMessage("Questionário deve ter pelo menos uma pergunta");
    }
}
