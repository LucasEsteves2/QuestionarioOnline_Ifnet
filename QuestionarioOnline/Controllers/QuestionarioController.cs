using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionarioOnline.Api.Responses;
using QuestionarioOnline.Application.DTOs.Requests;
using QuestionarioOnline.Application.DTOs.Responses;
using QuestionarioOnline.Application.Interfaces;
using System.Security.Claims;

namespace QuestionarioOnline.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class QuestionarioController : BaseController
{
    private readonly IQuestionarioService _questionarioService;

    public QuestionarioController(IQuestionarioService questionarioService)
    {
        _questionarioService = questionarioService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<QuestionarioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<QuestionarioDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<QuestionarioDto>>> Criar([FromBody] CriarQuestionarioRequest request, CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioIdDoToken();
        var result = await _questionarioService.CriarQuestionarioAsync(request, usuarioId, cancellationToken);

        if (result.IsFailure)
            return FailResponse(result);

        return CreatedResponse(nameof(ObterPorId), new { id = result.Value.Id }, result.Value);
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<QuestionarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<QuestionarioDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<QuestionarioDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<QuestionarioDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<QuestionarioDto>>> Encerrar(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioIdDoToken();
        var result = await _questionarioService.EncerrarQuestionarioAsync(id, usuarioId, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deletar(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioIdDoToken();
        var result = await _questionarioService.DeletarQuestionarioAsync(id, usuarioId, cancellationToken);
        
        if (result.IsFailure)
            return FailResponseNoContent(result.Error);

        return NoContent();
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<QuestionarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<QuestionarioDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<QuestionarioDto>>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioIdDoToken();
        var questionario = await _questionarioService.ObterQuestionarioPorIdAsync(id, usuarioId, cancellationToken);
        
        if (questionario is null)
            return NotFoundResponse<QuestionarioDto>();

        return OkResponse(questionario);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<QuestionarioListaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<QuestionarioListaDto>>>> Listar(CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioIdDoToken();
        var questionarios = await _questionarioService.ListarQuestionariosPorUsuarioAsync(usuarioId, cancellationToken);
        return OkResponse(questionarios);
    }

    [HttpGet("{id}/resultados")]
    [Authorize(Roles = "Admin,Analista,Visualizador")]
    [ProducesResponseType(typeof(ApiResponse<ResultadoQuestionarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ResultadoQuestionarioDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ResultadoQuestionarioDto>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ResultadoQuestionarioDto>>> ObterResultados(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioIdDoToken();
        var result = await _questionarioService.ObterResultadosAsync(id, usuarioId, cancellationToken);
        
        if (result.IsFailure)
            return NotFoundOrForbiddenResponse<ResultadoQuestionarioDto>(result.Error);

        return OkResponse(result.Value);
    }
}
