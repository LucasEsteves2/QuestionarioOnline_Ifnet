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

    protected ActionResult<ApiResponse<T>> FailResponse<T>(Result<T> result)
    {
        var response = ApiResponse<T>.Fail(result.Error);
        return BadRequest(response);
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

        if (message.Contains("não autorizado", StringComparison.OrdinalIgnoreCase))
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(message, statusCode: 403));

        return BadRequest(ApiResponse<object>.Fail(message));
    }

    protected ActionResult<ApiResponse<T>> NotFoundOrForbiddenResponse<T>(string message)
    {
        if (message.Contains("não autorizado", StringComparison.OrdinalIgnoreCase))
        {
            var response = ApiResponse<T>.Fail(message, statusCode: 403);
            return StatusCode(StatusCodes.Status403Forbidden, response);
        }

        return NotFoundResponse<T>(message);
    }

    protected ActionResult<ApiResponse<T>> UnauthorizedResponse<T>(string? message = null)
    {
        var response = ApiResponse<T>.Fail(message ?? "Não autorizado", statusCode: 401);
        return Unauthorized(response);
    }

    protected ActionResult<ApiResponse<T>> ErrorResponse<T>(string message)
    {
        var response = ApiResponse<T>.Error(message);
        return StatusCode(StatusCodes.Status500InternalServerError, response);
    }

    protected ActionResult<ApiResponse<T>> FromResult<T>(Result<T> result, string? successMessage = null)
    {
        if (result.IsSuccess)
            return OkResponse(result.Value, successMessage);

        return FailResponse(result);
    }

    protected Guid ObterUsuarioIdDoToken()
    {
        return User.ObterUsuarioId();
    }

}
