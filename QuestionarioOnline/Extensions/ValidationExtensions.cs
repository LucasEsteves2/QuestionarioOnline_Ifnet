using FluentValidation.Results;

namespace QuestionarioOnline.Api.Extensions;

/// <summary>
/// Extension methods para converter ValidationResult em ApiResponse
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Converte ValidationResult do FluentValidation para Dictionary<string, string[]>
    /// </summary>
    public static Dictionary<string, string[]> ToErrorDictionary(this ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );
    }
}
