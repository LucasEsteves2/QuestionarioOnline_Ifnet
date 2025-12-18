# ? Reorganização: Um Arquivo por Contract

## ?? Mudança Aplicada

**Cada Request/Response agora tem seu próprio arquivo!**

---

## ?? Estrutura Final

```
QuestionarioOnline.Api/
??? Contracts/
?   ??? Requests/
?   ?   ??? CriarQuestionarioRequest.cs          (+ CriarPerguntaRequest, CriarOpcaoRequest)
?   ?   ??? QuestionarioRequestExtensions.cs
?   ?   ??? RegistrarRespostaRequest.cs          (+ RespostaItemRequest)
?   ?   ??? RegistrarUsuarioRequest.cs
?   ?   ??? LoginRequest.cs
?   ??? Responses/
?       ??? QuestionarioResponse.cs
?       ??? PerguntaResponse.cs
?       ??? OpcaoResponse.cs
?       ??? QuestionarioListaResponse.cs
?       ??? QuestionarioPublicoResponse.cs
?       ??? ResultadoQuestionarioResponse.cs
?       ??? ResultadoPerguntaResponse.cs
?       ??? ResultadoOpcaoResponse.cs
?       ??? RespostaRegistradaResponse.cs
?       ??? UsuarioRegistradoResponse.cs
?       ??? LoginResponse.cs
```

---

## ?? Antes vs Depois

### ? **ANTES - Tudo Junto**

```
Responses/
  ??? QuestionarioResponse.cs       (QuestionarioResponse + PerguntaResponse + OpcaoResponse)
  ??? ResultadoQuestionarioResponse.cs   (3 records no mesmo arquivo)
  ??? AuthResponses.cs              (UsuarioRegistradoResponse + LoginResponse)

Requests/
  ??? CriarQuestionarioRequest.cs   (3 records no mesmo arquivo)
  ??? RegistrarRespostaRequest.cs   (2 records no mesmo arquivo)
  ??? AuthRequests.cs               (2 records no mesmo arquivo)
```

**Problemas:**
- ?? Difícil encontrar um record específico
- ?? Arquivo grande com múltiplos records
- ?? Menos organizado

---

### ? **DEPOIS - Um Arquivo por Contract Principal**

```
Responses/
  ??? QuestionarioResponse.cs              ? Apenas QuestionarioResponse
  ??? PerguntaResponse.cs                  ? Separado
  ??? OpcaoResponse.cs                     ? Separado
  ??? QuestionarioListaResponse.cs         ? Apenas QuestionarioListaResponse
  ??? QuestionarioPublicoResponse.cs       ? Apenas QuestionarioPublicoResponse
  ??? ResultadoQuestionarioResponse.cs     ? Apenas ResultadoQuestionarioResponse
  ??? ResultadoPerguntaResponse.cs         ? Separado
  ??? ResultadoOpcaoResponse.cs            ? Separado
  ??? RespostaRegistradaResponse.cs        ? Apenas RespostaRegistradaResponse
  ??? UsuarioRegistradoResponse.cs         ? Separado
  ??? LoginResponse.cs                     ? Separado

Requests/
  ??? CriarQuestionarioRequest.cs          ? Principal + sub-records
  ??? RegistrarRespostaRequest.cs          ? Principal + sub-records
  ??? RegistrarUsuarioRequest.cs           ? Separado
  ??? LoginRequest.cs                      ? Separado
```

**Benefícios:**
- ? Fácil encontrar qualquer contract
- ? Arquivos menores e focados
- ? Melhor organização
- ? IntelliSense mais preciso

---

## ?? Exceções: Sub-Records no Mesmo Arquivo

### 1. **CriarQuestionarioRequest.cs**

```csharp
namespace QuestionarioOnline.Api.Contracts.Requests;

public record CriarQuestionarioRequest(...)    // Principal

public record CriarPerguntaRequest(...)        // Sub-record (usado apenas pelo principal)

public record CriarOpcaoRequest(...)           // Sub-record (usado apenas pelo principal)
```

**Por quê junto?**
- CriarPerguntaRequest e CriarOpcaoRequest são **usados APENAS** por CriarQuestionarioRequest
- Não fazem sentido isolados
- Forte acoplamento conceitual

### 2. **RegistrarRespostaRequest.cs**

```csharp
namespace QuestionarioOnline.Api.Contracts.Requests;

public record RegistrarRespostaRequest(...)    // Principal

public record RespostaItemRequest(...)         // Sub-record (usado apenas pelo principal)
```

**Por quê junto?**
- RespostaItemRequest é **usado APENAS** por RegistrarRespostaRequest
- Não faz sentido isolado

---

## ?? Regra de Organização

### ? **Arquivo Separado SE:**
- Record é usado em **múltiplos lugares**
- Record representa um **conceito independente**
- Record pode ser **retornado diretamente** por um endpoint

**Exemplos:**
- `PerguntaResponse` - Usado por QuestionarioResponse E QuestionarioPublicoResponse
- `OpcaoResponse` - Usado por PerguntaResponse
- `LoginResponse` - Retornado diretamente por POST /api/auth/login

### ? **Mesmo Arquivo SE:**
- Sub-record é usado **APENAS** pelo record principal
- Sub-record NÃO faz sentido **sozinho**
- Forte **acoplamento conceitual**

**Exemplos:**
- `CriarPerguntaRequest` - Usado APENAS por CriarQuestionarioRequest
- `CriarOpcaoRequest` - Usado APENAS por CriarPerguntaRequest
- `RespostaItemRequest` - Usado APENAS por RegistrarRespostaRequest

---

## ?? Estatísticas

| Métrica | Antes | Depois |
|---------|-------|--------|
| **Arquivos Responses** | 5 | 11 |
| **Arquivos Requests** | 3 | 5 |
| **Records por arquivo** | 2-3 | 1-3 |
| **Summary comments** | Todos | Removidos |
| **Facilidade de encontrar** | Média | Alta ? |

---

## ??? Mapeamento Completo

### Responses

| Arquivo | Records |
|---------|---------|
| `QuestionarioResponse.cs` | `QuestionarioResponse` |
| `PerguntaResponse.cs` | `PerguntaResponse` |
| `OpcaoResponse.cs` | `OpcaoResponse` |
| `QuestionarioListaResponse.cs` | `QuestionarioListaResponse` |
| `QuestionarioPublicoResponse.cs` | `QuestionarioPublicoResponse` |
| `ResultadoQuestionarioResponse.cs` | `ResultadoQuestionarioResponse` |
| `ResultadoPerguntaResponse.cs` | `ResultadoPerguntaResponse` |
| `ResultadoOpcaoResponse.cs` | `ResultadoOpcaoResponse` |
| `RespostaRegistradaResponse.cs` | `RespostaRegistradaResponse` |
| `UsuarioRegistradoResponse.cs` | `UsuarioRegistradoResponse` |
| `LoginResponse.cs` | `LoginResponse` |

### Requests

| Arquivo | Records |
|---------|---------|
| `CriarQuestionarioRequest.cs` | `CriarQuestionarioRequest`<br>`CriarPerguntaRequest`<br>`CriarOpcaoRequest` |
| `QuestionarioRequestExtensions.cs` | Extensões de mapeamento |
| `RegistrarRespostaRequest.cs` | `RegistrarRespostaRequest`<br>`RespostaItemRequest` |
| `RegistrarUsuarioRequest.cs` | `RegistrarUsuarioRequest` |
| `LoginRequest.cs` | `LoginRequest` |

---

## ?? Padrão de Código Limpo

### Sem Summary Comments

```csharp
// ? ANTES - Verboso
/// <summary>
/// Response completo de um questionário (API Contract)
/// </summary>
public record QuestionarioResponse(...)

// ? DEPOIS - Limpo
public record QuestionarioResponse(...)
```

**Por quê remover?**
- Nome do record já é auto-explicativo
- Summary não adiciona informação útil
- Código mais limpo e conciso

### Estrutura Consistente

```csharp
using QuestionarioOnline.Application.DTOs.Responses;

namespace QuestionarioOnline.Api.Contracts.Responses;

public record QuestionarioResponse(...)
{
    public static QuestionarioResponse From(QuestionarioDto dto) => new(...);
}
```

**Padrão em TODOS os Responses:**
1. Using do Application.DTOs
2. Namespace correto
3. Record com propriedades
4. Método estático `From()` para mapeamento

---

## ? Conclusão

**Reorganização completa para um arquivo por contract:**

? **11 arquivos Responses** - Um por conceito  
? **5 arquivos Requests** - Sub-records apenas quando necessário  
? **Sem summary** - Código mais limpo  
? **Fácil navegar** - IntelliSense preciso  
? **Manutenção simples** - Encontrar qualquer contract rapidamente  

**Estrutura profissional e organizada!** ??
