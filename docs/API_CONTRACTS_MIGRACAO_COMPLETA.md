# ? Migração Completa: API Contracts Separados

## ?? Migração Implementada

**API Contracts agora separados dos Application DTOs!**

---

## ?? Nova Estrutura

```
QuestionarioOnline.Api/
??? Contracts/
?   ??? Requests/
?   ?   ??? CriarQuestionarioRequest.cs           ? API Contract
?   ?   ??? QuestionarioRequestExtensions.cs      ? Mapeamento
?   ?   ??? RegistrarRespostaRequest.cs           ? API Contract
?   ?   ??? AuthRequests.cs                       ? Login + Register
?   ??? Responses/
?       ??? QuestionarioResponse.cs               ? Completo
?       ??? QuestionarioListaResponse.cs          ? Lista simplificada
?       ??? QuestionarioPublicoResponse.cs        ? Público (sem auth)
?       ??? ResultadoQuestionarioResponse.cs      ? Resultados
?       ??? RespostaRegistradaResponse.cs         ? Confirmação
?       ??? AuthResponses.cs                      ? Login + Register
??? Controllers/
    ??? QuestionarioController.cs                 ? Mapeia Contracts
    ??? RespostaController.cs                     ? Mapeia Contracts
    ??? AuthController.cs                         ? Mapeia Contracts

QuestionarioOnline.Application/
??? DTOs/
    ??? Requests/                                 ? Permanecem intactos
    ??? Responses/                                ? Permanecem intactos
```

---

## ?? Fluxo de Mapeamento

### Request (Cliente ? API ? Application)

```
Cliente envia JSON
    ?
API Contract (CriarQuestionarioRequest)
    ? .ToApplicationDto()
Application DTO (CriarQuestionarioRequest)
    ?
Service processa
```

### Response (Application ? API ? Cliente)

```
Service retorna DTO
    ?
Application DTO (QuestionarioDto)
    ? QuestionarioResponse.From(dto)
API Contract (QuestionarioResponse)
    ?
Cliente recebe JSON
```

---

## ?? Contracts Criados

### 1. **QuestionarioRequest** (POST)

```csharp
// API/Contracts/Requests/CriarQuestionarioRequest.cs
public record CriarQuestionarioRequest(
    string Titulo,
    string? Descricao,
    DateTime DataInicio,
    DateTime DataFim,
    List<CriarPerguntaRequest> Perguntas
);

// Mapeamento
public static class QuestionarioRequestExtensions
{
    public static AppDto.CriarQuestionarioRequest ToApplicationDto(this CriarQuestionarioRequest request)
    {
        var perguntas = request.Perguntas.Select(p => new AppDto.CriarPerguntaDto(...)).ToList();
        return new AppDto.CriarQuestionarioRequest(request.Titulo, ...);
    }
}
```

**Uso no Controller:**

```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<QuestionarioResponse>>> Criar([FromBody] CriarQuestionarioRequest request)
{
    var usuarioId = ObterUsuarioIdDoToken();
    var applicationDto = request.ToApplicationDto(); // ? Mapeia
    var result = await _questionarioService.CriarQuestionarioAsync(applicationDto, usuarioId);
    
    if (result.IsFailure)
        return FailResponse<QuestionarioResponse>(result.Error);

    var response = QuestionarioResponse.From(result.Value); // ? Mapeia
    return OkResponse(response);
}
```

---

### 2. **QuestionarioResponse** (GET completo)

```csharp
// API/Contracts/Responses/QuestionarioResponse.cs
public record QuestionarioResponse(
    Guid Id,
    string Titulo,
    string? Descricao,
    string Status,
    DateTime DataInicio,
    DateTime DataFim,
    DateTime DataCriacao,
    DateTime? DataEncerramento,
    List<PerguntaResponse> Perguntas
)
{
    public static QuestionarioResponse From(QuestionarioDto dto) => new(
        dto.Id,
        dto.Titulo,
        dto.Descricao,
        dto.Status,
        dto.DataInicio,
        dto.DataFim,
        dto.DataCriacao,
        dto.DataEncerramento,
        dto.Perguntas.Select(PerguntaResponse.From).ToList()
    );
}
```

**Uso no Controller:**

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<QuestionarioResponse>>> ObterPorId(Guid id)
{
    var result = await _questionarioService.ObterQuestionarioPorIdAsync(id);
    
    if (result.IsFailure)
        return FailResponse<QuestionarioResponse>(result.Error);

    var response = QuestionarioResponse.From(result.Value); // ? Mapeia
    return OkResponse(response);
}
```

---

### 3. **QuestionarioListaResponse** (GET lista)

```csharp
// API/Contracts/Responses/QuestionarioListaResponse.cs
public record QuestionarioListaResponse(
    Guid Id,
    string Titulo,
    string Status,
    DateTime DataInicio,
    DateTime DataFim,
    int TotalPerguntas
)
{
    public static QuestionarioListaResponse From(QuestionarioListaDto dto) => new(
        dto.Id,
        dto.Titulo,
        dto.Status,
        dto.DataInicio,
        dto.DataFim,
        dto.TotalPerguntas
    );
}
```

**Uso no Controller:**

```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<IEnumerable<QuestionarioListaResponse>>>> Listar()
{
    var dtos = await _questionarioService.ListarTodosQuestionariosAsync();
    var responses = dtos.Select(QuestionarioListaResponse.From); // ? Mapeia cada item
    return OkResponse(responses);
}
```

---

### 4. **ResultadoQuestionarioResponse** (GET resultados)

```csharp
// API/Contracts/Responses/ResultadoQuestionarioResponse.cs
public record ResultadoQuestionarioResponse(
    Guid Id,
    string Titulo,
    int TotalRespostas,
    List<ResultadoPerguntaResponse> Perguntas
)
{
    public static ResultadoQuestionarioResponse From(ResultadoQuestionarioDto dto) => new(
        dto.QuestionarioId, // ? Mapeia propriedades diferentes
        dto.Titulo,
        dto.TotalRespostas,
        dto.Perguntas.Select(ResultadoPerguntaResponse.From).ToList()
    );
}

public record ResultadoPerguntaResponse(
    Guid Id,
    string Texto,
    List<ResultadoOpcaoResponse> Opcoes
)
{
    public static ResultadoPerguntaResponse From(ResultadoPerguntaDto dto) => new(
        dto.PerguntaId, // ? Mapeia
        dto.Texto,
        dto.Opcoes.Select(ResultadoOpcaoResponse.From).ToList()
    );
}

public record ResultadoOpcaoResponse(
    Guid Id,
    string Texto,
    int Votos,
    double Percentual
)
{
    public static ResultadoOpcaoResponse From(ResultadoOpcaoDto dto) => new(
        dto.OpcaoId,      // ? Mapeia
        dto.Texto,
        dto.TotalVotos,   // ? Mapeia
        dto.Percentual
    );
}
```

---

### 5. **RegistrarRespostaRequest** (POST resposta)

```csharp
// API/Contracts/Requests/RegistrarRespostaRequest.cs
public record RegistrarRespostaRequest(
    Guid QuestionarioId,
    List<RespostaItemRequest> Respostas,
    string? Estado = null,
    string? Cidade = null,
    string? RegiaoGeografica = null
)
{
    public AppDto.RegistrarRespostaRequest ToApplicationDto() => new(
        QuestionarioId,
        Respostas.Select(r => new AppDto.RespostaItemDto(r.PerguntaId, r.OpcaoRespostaId)).ToList(),
        Estado,
        Cidade,
        RegiaoGeografica
    );
}
```

**Uso no Controller:**

```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<RespostaRegistradaResponse>>> Registrar([FromBody] RegistrarRespostaRequest request)
{
    var applicationDto = request.ToApplicationDto(); // ? Mapeia
    var result = await _respostaService.RegistrarRespostaAsync(applicationDto);

    if (result.IsFailure)
        return FailResponse<RespostaRegistradaResponse>(result.Error);

    var response = RespostaRegistradaResponse.From(result.Value); // ? Mapeia
    return AcceptedResponse(response);
}
```

---

### 6. **AuthRequests** (Login + Register)

```csharp
// API/Contracts/Requests/AuthRequests.cs
public record RegistrarUsuarioRequest(string Nome, string Email, string Senha)
{
    public AppDto.RegistrarUsuarioRequest ToApplicationDto() => new(Nome, Email, Senha);
}

public record LoginRequest(string Email, string Senha)
{
    public AppDto.LoginRequest ToApplicationDto() => new(Email, Senha);
}
```

**Uso no Controller:**

```csharp
[HttpPost("register")]
public async Task<ActionResult<ApiResponse<UsuarioRegistradoResponse>>> Register([FromBody] RegistrarUsuarioRequest request)
{
    var applicationDto = request.ToApplicationDto(); // ? Mapeia
    var result = await _authService.RegistrarAsync(applicationDto);

    if (result.IsFailure)
        return FailResponse<UsuarioRegistradoResponse>(result.Error);

    var response = UsuarioRegistradoResponse.From(result.Value); // ? Mapeia
    return StatusCode(201, ApiResponse<UsuarioRegistradoResponse>.Success(response, 201));
}
```

---

## ?? Comparação: Antes vs Depois

### Antes (DTOs Compartilhados)

```csharp
// Controller usava DTO da Application diretamente
[HttpPost]
public async Task<ActionResult<ApiResponse<QuestionarioDto>>> Criar([FromBody] CriarQuestionarioRequest request)
{
    var result = await _service.CriarQuestionarioAsync(request, usuarioId);
    return FromResult(result); // ? Direto
}
```

**Problemas:**
- ? API acoplada aos DTOs internos
- ? Difícil versionar API (v1, v2)
- ? Se DTO mudar, API muda junto

---

### Depois (API Contracts)

```csharp
// Controller usa Contract da API e mapeia
[HttpPost]
public async Task<ActionResult<ApiResponse<QuestionarioResponse>>> Criar([FromBody] CriarQuestionarioRequest request)
{
    var applicationDto = request.ToApplicationDto();                   // ? Mapeia Request
    var result = await _service.CriarQuestionarioAsync(applicationDto, usuarioId);
    
    if (result.IsFailure)
        return FailResponse<QuestionarioResponse>(result.Error);
    
    var response = QuestionarioResponse.From(result.Value);           // ? Mapeia Response
    return OkResponse(response);
}
```

**Benefícios:**
- ? API desacoplada dos DTOs internos
- ? Contratos de API estáveis
- ? Fácil versionar (v1, v2 com Responses diferentes)
- ? Controle total sobre o que expor

---

## ?? Padrão de Mapeamento

### Método Static `From()`

```csharp
public record QuestionarioResponse(...)
{
    public static QuestionarioResponse From(QuestionarioDto dto) => new(...);
    //            ? Método estático para criar Response a partir do DTO
}
```

**Por quê static?**
- ? Sem necessidade de instanciar
- ? Chamada simples: `QuestionarioResponse.From(dto)`
- ? Padrão Factory Method

### Método de Extensão `ToApplicationDto()`

```csharp
public static class QuestionarioRequestExtensions
{
    public static AppDto.CriarQuestionarioRequest ToApplicationDto(this CriarQuestionarioRequest request)
    {
        // Mapeamento complexo
    }
}
```

**Por quê extensão?**
- ? Sintaxe fluente: `request.ToApplicationDto()`
- ? Separação de responsabilidades
- ? Fácil descobrir (IntelliSense)

---

## ?? Diferenças de Nomes (DTO vs Response)

| Application DTO | API Response | Motivo |
|-----------------|--------------|--------|
| `QuestionarioId` | `Id` | Cliente não precisa saber que é "QuestionarioId" |
| `PerguntaId` | `Id` | Mais limpo na API |
| `OpcaoId` | `Id` | Consistência |
| `TotalVotos` | `Votos` | Mais curto e direto |
| `UsuarioId` | `Id` | Contexto já define que é usuário |
| `DataResposta` | `DataRegistro` | Mais claro para cliente |

**Benefício:** API pública pode ter nomes mais simples/claros que DTOs internos

---

## ?? Checklist de Migração

### ? Requests

- ? `CriarQuestionarioRequest` com sub-records (`CriarPerguntaRequest`, `CriarOpcaoRequest`)
- ? `RegistrarRespostaRequest` com `RespostaItemRequest`
- ? `RegistrarUsuarioRequest`
- ? `LoginRequest`

### ? Responses

- ? `QuestionarioResponse` (completo)
- ? `QuestionarioListaResponse` (simplificado)
- ? `QuestionarioPublicoResponse` (público)
- ? `ResultadoQuestionarioResponse` com sub-records
- ? `RespostaRegistradaResponse`
- ? `UsuarioRegistradoResponse`
- ? `LoginResponse`

### ? Controllers

- ? `QuestionarioController` - Todos endpoints mapeando
- ? `RespostaController` - Mapeia Request e Response
- ? `AuthController` - Mapeia Login e Register

### ? Mapeamentos

- ? Método `From()` estático em todos Responses
- ? Método `ToApplicationDto()` em Requests complexos
- ? Classe `QuestionarioRequestExtensions` para mapeamento elaborado

---

## ? Vantagens Obtidas

### 1. **Contratos de API Estáveis**

```csharp
// DTO pode mudar internamente
public record QuestionarioDto(Guid Id, string Titulo, string CampoNovo);

// Response continua igual (não quebra clientes)
public record QuestionarioResponse(Guid Id, string Titulo); // ? Sem CampoNovo
```

### 2. **Versionamento Fácil**

```csharp
// v1/Responses/QuestionarioResponseV1.cs
public record QuestionarioResponseV1(Guid Id, string Titulo);

// v2/Responses/QuestionarioResponseV2.cs
public record QuestionarioResponseV2(Guid Id, string Titulo, string Status);

// Ambos mapeiam do mesmo DTO
```

### 3. **Controle de Exposição**

```csharp
// DTO interno
public record UsuarioDto(Guid Id, string Nome, string SenhaHash);

// Response API (sem SenhaHash)
public record UsuarioResponse(Guid Id, string Nome)
{
    public static UsuarioResponse From(UsuarioDto dto) => new(dto.Id, dto.Nome); // ? Não expõe SenhaHash
}
```

### 4. **Swagger/OpenAPI Limpo**

```yaml
# ANTES
components:
  schemas:
    QuestionarioDto:  # ? Nome "Dto" vaza

# DEPOIS
components:
  schemas:
    QuestionarioResponse:  # ? Nome semântico
```

---

## ? Conclusão

**API Contracts separados implementados com sucesso:**

? **Contratos estáveis** - API desacoplada dos DTOs internos  
? **Versionamento fácil** - Criar v1, v2 sem quebrar clientes  
? **Controle total** - Expor apenas o necessário  
? **Mapeamento limpo** - `From()` e `ToApplicationDto()`  
? **Build successful** - Tudo funcionando  
? **Pronto para produção** - Arquitetura profissional  

**Clean Architecture aplicada corretamente!** ??
