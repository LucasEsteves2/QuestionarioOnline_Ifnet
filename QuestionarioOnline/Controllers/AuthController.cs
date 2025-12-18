using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionarioOnline.Api.Responses;
using QuestionarioOnline.Application.DTOs.Requests;
using QuestionarioOnline.Application.DTOs.Responses;
using QuestionarioOnline.Application.Interfaces;

namespace QuestionarioOnline.Api.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UsuarioRegistradoDto>>> Register([FromBody] RegistrarUsuarioRequest request)
    {
        var result = await _authService.RegistrarAsync(request);

        if (result.IsFailure)
            return FailResponse<UsuarioRegistradoDto>(result.Error);

        var response = ApiResponse<UsuarioRegistradoDto>.Success(result.Value, statusCode: 201);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result.IsFailure)
            return UnauthorizedResponse<LoginResponse>(result.Error);

        return OkResponse(result.Value);
    }
}