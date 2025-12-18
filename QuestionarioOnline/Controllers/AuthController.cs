using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionarioOnline.Api.Contracts.Requests;
using QuestionarioOnline.Api.Contracts.Responses;
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
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Register([FromBody] RegistrarUsuarioRequest request)
    {
        var applicationDto = request.ToApplicationDto();
        var result = await _authService.RegistrarAsync(applicationDto);

        if (result.IsFailure)
            return FailResponse<LoginResponse>(result.Error);

        var response = LoginResponse.From(result.Value);
        return CreatedResponse(nameof(Register), new { id = response.UsuarioId }, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var applicationDto = request.ToApplicationDto();
        var result = await _authService.LoginAsync(applicationDto);

        if (result.IsFailure)
            return UnauthorizedResponse<LoginResponse>(result.Error);

        var response = LoginResponse.From(result.Value);
        return OkResponse(response);
    }
}