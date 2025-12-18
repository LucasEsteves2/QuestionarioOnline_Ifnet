using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionarioOnline.Api.Contracts.Responses;
using QuestionarioOnline.Api.Extensions;
using QuestionarioOnline.Domain.ValueObjects;

namespace QuestionarioOnline.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null)
    {
        var response = ApiResponse<T>.Success(data, message);
        return Ok(response);
    }

    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(string actionName, object routeValues, T data, string? message = null)
    {
        var response = ApiResponse<T>.Success(data, message, statusCode: 201);
        return CreatedAtAction(actionName, routeValues, response);
    }

    protected ActionResult<ApiResponse<T>> AcceptedResponse<T>(T data, string? message = null)
    {
        var response = ApiResponse<T>.Success(data, message, statusCode: 202);
        return Accepted(response);
    }

    protected ActionResult<ApiResponse<T>> FailResponse<T>(string message)
    {
        var response = ApiResponse<T>.Fail(message);
        return BadRequest(response);
    }

    protected ActionResult<ApiResponse<T>> NotFoundResponse<T>(string? message = null)
    {
        var response = ApiResponse<T>.NotFound(message);
        return NotFound(response);
    }

    protected IActionResult FailResponseNoContent(string message)
    {
        if (message.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
            return NotFound(ApiResponse<object>.NotFound(message));

        return BadRequest(ApiResponse<object>.Fail(message));
    }

    protected ActionResult<ApiResponse<T>> UnauthorizedResponse<T>(string message)
    {
        var response = ApiResponse<T>.Fail(message);
        return Unauthorized(response);
    }

    protected Guid ObterUsuarioIdDoToken()
    {
        return User.ObterUsuarioId();
    }
}
