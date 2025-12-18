using Microsoft.AspNetCore.Mvc;
using QuestionarioOnline.Api.Requests;
using QuestionarioOnline.Api.Responses;
using QuestionarioOnline.Application.DTOs.Requests;
using QuestionarioOnline.Application.DTOs.Responses;
using QuestionarioOnline.Application.Interfaces;

namespace QuestionarioOnline.Api.Controllers;

[Route("api/[controller]")]
public class RespostaController : BaseController
{
    private readonly IRespostaService _respostaService;

    public RespostaController(IRespostaService respostaService)
    {
        _respostaService = respostaService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RespostaRegistradaDto>>> Registrar([FromBody] RegistrarRespostaApiRequest request)
    {
        var applicationRequest = MapearParaApplicationRequest(request);

        var result = await _respostaService.RegistrarRespostaAsync(applicationRequest);

        if (result.IsFailure)
            return FailResponse(result);

        return AcceptedResponse(result.Value, "Resposta recebida e será processada em breve");
    }

    private static RegistrarRespostaRequest MapearParaApplicationRequest(RegistrarRespostaApiRequest apiRequest)
    {
        var respostas = apiRequest.Respostas
            .Select(r => new RespostaItemDto(r.PerguntaId, r.OpcaoRespostaId))
            .ToList();

        return new RegistrarRespostaRequest(
            apiRequest.QuestionarioId,
            respostas,
            apiRequest.Estado,
            apiRequest.Cidade,
            apiRequest.RegiaoGeografica
        );
    }
}
