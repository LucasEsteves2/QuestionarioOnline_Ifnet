using QuestionarioOnline.Application.DTOs.Requests;
using QuestionarioOnline.Application.DTOs.Responses;
using QuestionarioOnline.Application.Interfaces;
using QuestionarioOnline.Application.Validators;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Enums;
using QuestionarioOnline.Domain.Exceptions;
using QuestionarioOnline.Domain.Interfaces;
using QuestionarioOnline.Domain.ValueObjects;

namespace QuestionarioOnline.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly RegistrarUsuarioRequestValidator _registrarValidator;
    private readonly LoginRequestValidator _loginValidator;

    public AuthService(IUsuarioRepository usuarioRepository, IJwtTokenService jwtTokenService, RegistrarUsuarioRequestValidator registrarValidator, LoginRequestValidator loginValidator)
    {
        _usuarioRepository = usuarioRepository;
        _jwtTokenService = jwtTokenService;
        _registrarValidator = registrarValidator;
        _loginValidator = loginValidator;
    }

    public async Task<Result<LoginResponse>> RegistrarAsync(RegistrarUsuarioRequest request)
    {
        var validationResult = await _registrarValidator.ValidateAsync(request);
     
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<LoginResponse>($"Erro de validação: {errors}");
        }

        try
        {
            var email = Email.Create(request.Email);

            var usuarioExistente = await _usuarioRepository.ObterPorEmailAsync(email);
     
            if (usuarioExistente != null)
                throw new DomainException($"O email '{request.Email}' já está cadastrado no sistema");

            var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);
            var novoUsuario = new Usuario(request.Nome, email, senhaHash, UsuarioRole.Admin);

            await _usuarioRepository.AdicionarAsync(novoUsuario);

            var token = _jwtTokenService.GerarToken(novoUsuario);
            var response = new LoginResponse(token, novoUsuario.Id, novoUsuario.Nome, novoUsuario.Email.Address);
            
            return Result.Success(response);
        }
        catch (DomainException ex)
        {
            return Result.Failure<LoginResponse>(ex.Message);
        }
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<LoginResponse>($"Erro de validação: {errors}");
        }

        try
        {
            var email = Email.Create(request.Email);

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null)
                return Result.Failure<LoginResponse>("Email ou senha inválidos");

            usuario.GarantirQueEstaAtivo();

            var senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash);
            if (!senhaValida)
                return Result.Failure<LoginResponse>("Email ou senha inválidos");

            var token = _jwtTokenService.GerarToken(usuario);
            var response = new LoginResponse(token, usuario.Id, usuario.Nome, usuario.Email.Address);

            return Result.Success(response);
        }
        catch (DomainException ex)
        {
            return Result.Failure<LoginResponse>(ex.Message);
        }
    }
}
