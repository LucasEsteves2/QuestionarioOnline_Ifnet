using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionarioOnline.Api.Contracts.Responses;
using QuestionarioOnline.Application.DTOs.Requests;
using QuestionarioOnline.Application.Interfaces;
using ApiRequest = QuestionarioOnline.Api.Contracts.Requests;

namespace QuestionarioOnline.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class RespostaController : BaseController
{
    private readonly IRespostaService _respostaService;

    public RespostaController(IRespostaService respostaService)
    {
        _respostaService = respostaService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<RespostaRegistradaResponse>>> Registrar([FromBody] ApiRequest.RegistrarRespostaRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString() ?? "Unknown";

        var applicationDto = new RegistrarRespostaRequestDto(
            request.QuestionarioId,
            request.Respostas.Select(r => new RespostaItemDto(r.PerguntaId, r.OpcaoRespostaId)).ToList(),
            ipAddress,
            userAgent,
            request.Estado,
            request.Cidade,
            request.RegiaoGeografica
        );

        var result = await _respostaService.RegistrarRespostaAsync(applicationDto);

        if (result.IsNotFound)
            return NotFoundResponse<RespostaRegistradaResponse>(result.Error);

        if (result.IsFailure)
            return FailResponse<RespostaRegistradaResponse>(result.Error);

        var response = RespostaRegistradaResponse.From(result.Value);
        return AcceptedResponse(response, "Resposta recebida e será processada em breve");
    }

    [HttpGet("questionario/{questionarioId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RespostaResponse>>>> ObterPorQuestionario(Guid questionarioId)
    {
        var result = await _respostaService.ObterRespostasPorQuestionarioAsync(questionarioId);

        if (result.IsNotFound)
            return NotFoundResponse<IEnumerable<RespostaResponse>>(result.Error);

        if (result.IsFailure)
            return FailResponse<IEnumerable<RespostaResponse>>(result.Error);

        var response = result.Value.Select(RespostaResponse.From);
        return OkResponse(response);
    }
}
