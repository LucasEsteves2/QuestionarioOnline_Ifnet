# ? Correção: 404 Not Found para Recursos Não Encontrados

## ?? Problema Identificado e Corrigido

**Quando um recurso não existe, agora retornamos `404 Not Found` ao invés de `400 Bad Request`!**

---

## ?? Antes vs Depois

### ? **ANTES - Status Code Incorreto**

```csharp
// QuestionarioService.cs
private async Task<Questionario> ObterQuestionarioAsync(Guid questionarioId, ...)
{
    var questionario = await _questionarioRepository.ObterPorIdComPerguntasAsync(questionarioId, ...);

    if (questionario is null)
        throw new DomainException("Questionário não encontrado"); // ? DomainException genérica
    
    return questionario;
}

// Service
catch (DomainException ex)
{
    return Result.Failure<QuestionarioDto>(ex.Message); // ? Failure genérico
}

// BaseController
protected ActionResult<ApiResponse<T>> FailResponse<T>(Result<T> result)
{
    return BadRequest(response); // ? Sempre 400 Bad Request
}
```

**Response HTTP:**
```http
HTTP/1.1 400 Bad Request  ? ? Status code errado!
Content-Type: application/json

{
  "success": false,
  "error": "Questionário não encontrado"
}
```

**Problema:**
- ? `400 Bad Request` indica **erro do cliente** (dados inválidos)
- ? Recurso não encontrado deveria ser `404 Not Found`
- ? Semântica HTTP incorreta

---

### ? **DEPOIS - Status Code Correto**

#### 1. **Nova Exceção: NotFoundException**

```csharp
// QuestionarioOnline.Domain/Exceptions/NotFoundException.cs
namespace QuestionarioOnline.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message)
    {
    }
}
```

**Por quê herdar de DomainException?**
- ? Mantém hierarquia (é um tipo de DomainException)
- ? Fácil identificar (catch específico)
- ? Semântica clara (não encontrado ? erro de validação)

---

#### 2. **Result com Flag IsNotFound**

```csharp
// QuestionarioOnline.Domain/ValueObjects/Result.cs
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public bool IsNotFound { get; } // ? Flag para 404

    protected Result(bool isSuccess, string error, List<string>? errors = null, bool isNotFound = false)
    {
        // ...
        IsNotFound = isNotFound;
    }

    public static Result NotFound(string error) => new(false, error, isNotFound: true); // ? Método factory

    public static Result<T> NotFound<T>(string error) => new(default!, false, error, isNotFound: true);
}
```

**Benefícios:**
- ? `Result.NotFound()` indica explicitamente 404
- ? Separado de `Result.Failure()` (400)
- ? Type-safe (flag booleana)

---

#### 3. **Service Lança NotFoundException**

```csharp
// QuestionarioService.cs
private async Task<Questionario> ObterQuestionarioAsync(Guid questionarioId, ...)
{
    var questionario = await _questionarioRepository.ObterPorIdComPerguntasAsync(questionarioId, ...);

    if (questionario is null)
        throw new NotFoundException("Questionário não encontrado"); // ? Exceção específica
    
    return questionario;
}

// Métodos públicos tratam NotFoundException
public async Task<Result<QuestionarioDto>> ObterQuestionarioPorIdAsync(Guid questionarioId, ...)
{
    try
    {
        var questionario = await ObterQuestionarioAsync(questionarioId, cancellationToken);
        return Result.Success(QuestionarioMapper.ToDto(questionario));
    }
    catch (NotFoundException ex)
    {
        return Result.NotFound<QuestionarioDto>(ex.Message); // ? Result.NotFound
    }
    catch (DomainException ex)
    {
        return Result.Failure<QuestionarioDto>(ex.Message); // 400
    }
}
```

**Catch Order Importante:**
1. `NotFoundException` primeiro (mais específica)
2. `DomainException` depois (mais genérica)

---

#### 4. **BaseController Retorna 404**

```csharp
// BaseController.cs
protected ActionResult<ApiResponse<T>> FailResponse<T>(Result<T> result)
{
    if (result.IsNotFound)
    {
        var notFoundResponse = ApiResponse<T>.NotFound(result.Error);
        return NotFound(notFoundResponse); // ? 404 Not Found
    }

    var response = ApiResponse<T>.Fail(result.Error);
    return BadRequest(response); // 400 Bad Request
}

protected ActionResult<ApiResponse<T>> FromResult<T>(Result<T> result)
{
    if (result.IsSuccess)
        return OkResponse(result.Value); // 200 OK

    if (result.IsNotFound)
        return NotFoundResponse<T>(result.Error); // ? 404 Not Found

    return FailResponse(result); // 400 Bad Request
}
```

**Lógica:**
1. Success ? `200 OK`
2. Not Found ? `404 Not Found`
3. Failure ? `400 Bad Request`

---

### ? **Response HTTP Correto**

```http
GET /api/questionario/abc-123-nao-existe

HTTP/1.1 404 Not Found  ? ? Status code correto!
Content-Type: application/json

{
  "success": false,
  "error": "Questionário não encontrado"
}
```

---

## ?? Fluxo Completo

### Request com ID Inválido

```
1. Cliente: GET /api/questionario/abc-123-inexistente
   ?
2. Controller: ObterPorId(id)
   ?
3. Service: ObterQuestionarioPorIdAsync(id)
   ?
4. Service: ObterQuestionarioAsync(id) ? null
   ?
5. Service: throw new NotFoundException("Questionário não encontrado")
   ?
6. Service: catch (NotFoundException ex) ? Result.NotFound<QuestionarioDto>(ex.Message)
   ?
7. Controller: FromResult(result) ? result.IsNotFound = true
   ?
8. Controller: return NotFoundResponse<T>(result.Error)
   ?
9. Cliente recebe: 404 Not Found
```

---

## ?? Status Codes por Cenário

| Cenário | Status Code | Quando Usar |
|---------|-------------|-------------|
| **Recurso encontrado** | `200 OK` | GET com sucesso |
| **Recurso criado** | `201 Created` | POST com sucesso |
| **Processamento assíncrono** | `202 Accepted` | POST de resposta (fila) |
| **Sem conteúdo** | `204 No Content` | DELETE com sucesso |
| **Dados inválidos** | `400 Bad Request` | Validação falhou, regra de negócio violada |
| **Não autenticado** | `401 Unauthorized` | Token ausente/inválido |
| **Sem permissão** | `403 Forbidden` | Usuário sem role necessária |
| **Recurso não encontrado** | `404 Not Found` | ? ID não existe no banco |
| **Erro do servidor** | `500 Internal Server Error` | Exception não tratada |

---

## ?? Exemplos de Uso

### ? **404 Not Found - Questionário Não Existe**

```http
GET /api/questionario/abc-123-inexistente
Authorization: Bearer {token}

Response:
HTTP/1.1 404 Not Found
{
  "success": false,
  "error": "Questionário não encontrado"
}
```

---

### ? **404 Not Found - Encerrar Questionário Inexistente**

```http
PATCH /api/questionario/abc-123-inexistente/status
Authorization: Bearer {token}

Response:
HTTP/1.1 404 Not Found
{
  "success": false,
  "error": "Questionário não encontrado"
}
```

---

### ? **404 Not Found - Deletar Questionário Inexistente**

```http
DELETE /api/questionario/abc-123-inexistente
Authorization: Bearer {token}

Response:
HTTP/1.1 404 Not Found
{
  "success": false,
  "error": "Questionário não encontrado"
}
```

---

### ? **400 Bad Request - Validação Falhou**

```http
POST /api/questionario
Authorization: Bearer {token}
Body:
{
  "titulo": "AB",  ? Menos de 3 caracteres
  "perguntas": []  ? Sem perguntas
}

Response:
HTTP/1.1 400 Bad Request
{
  "success": false,
  "error": "Erro de validação: Título deve ter entre 3 e 200 caracteres; Questionário deve ter pelo menos uma pergunta"
}
```

---

### ? **400 Bad Request - Regra de Negócio Violada**

```http
POST /api/resposta
Authorization: Bearer {token}
Body:
{
  "questionarioId": "abc-123",  ? Questionário encerrado
  "respostas": [...]
}

Response:
HTTP/1.1 400 Bad Request
{
  "success": false,
  "error": "O questionário não está ativo ou está fora do período de coleta"
}
```

---

## ?? Semântica HTTP Correta

### **400 Bad Request**
- ? **Dados inválidos** enviados pelo cliente
- ? **Validação falhou** (FluentValidation)
- ? **Regra de negócio violada** (questionário encerrado, etc.)

**Significado:** "Você enviou algo errado, corrija e tente novamente"

---

### **404 Not Found**
- ? **Recurso não existe** no banco de dados
- ? **ID inválido/inexistente**

**Significado:** "O que você está procurando não existe"

---

## ?? Comparação: 400 vs 404

| Cenário | Status Correto | Por Quê |
|---------|----------------|---------|
| Questionário com ID inexistente | `404 Not Found` | Recurso não existe |
| Título com 2 caracteres | `400 Bad Request` | Validação falhou |
| Questionário já encerrado | `400 Bad Request` | Regra de negócio violada |
| Email já cadastrado | `400 Bad Request` | Validação de unicidade |
| Usuário sem permissão | `403 Forbidden` | Sem role necessária |
| Token inválido | `401 Unauthorized` | Autenticação falhou |

---

## ? Benefícios Obtidos

### 1. **Semântica HTTP Correta**
```
404 Not Found = "Recurso não existe"
400 Bad Request = "Você enviou dados inválidos"
```

### 2. **Frontend Pode Diferenciar**
```typescript
try {
  const response = await api.get(`/questionario/${id}`);
} catch (error) {
  if (error.response.status === 404) {
    // Recurso não existe ? Redirecionar para lista
    navigate('/questionarios');
  } else if (error.response.status === 400) {
    // Erro de validação ? Mostrar mensagem
    showError(error.response.data.error);
  }
}
```

### 3. **RESTful Compliance**
```
RFC 7231 - HTTP/1.1 Semantics
"404 (Not Found) status code indicates that the origin server did not find a current representation for the target resource"
```

### 4. **Debugging Mais Fácil**
```
404 ? Problema de dados (ID errado)
400 ? Problema de validação/regra de negócio
500 ? Problema de código (bug)
```

---

## ?? Testando no Postman

### Teste 1: GET com ID Inexistente

```http
GET {{baseUrl}}/api/questionario/00000000-0000-0000-0000-000000000000
Authorization: Bearer {{authToken}}
```

**Response Esperado:**
```http
HTTP/1.1 404 Not Found

{
  "success": false,
  "error": "Questionário não encontrado"
}
```

---

### Teste 2: DELETE com ID Inexistente

```http
DELETE {{baseUrl}}/api/questionario/00000000-0000-0000-0000-000000000000
Authorization: Bearer {{authToken}}
```

**Response Esperado:**
```http
HTTP/1.1 404 Not Found

{
  "success": false,
  "error": "Questionário não encontrado"
}
```

---

### Teste 3: PATCH com ID Inexistente

```http
PATCH {{baseUrl}}/api/questionario/00000000-0000-0000-0000-000000000000/status
Authorization: Bearer {{authToken}}
```

**Response Esperado:**
```http
HTTP/1.1 404 Not Found

{
  "success": false,
  "error": "Questionário não encontrado"
}
```

---

## ? Checklist de Status Codes

### GET
- [x] Recurso encontrado ? `200 OK`
- [x] Recurso não encontrado ? `404 Not Found` ?

### POST
- [x] Recurso criado ? `201 Created`
- [x] Validação falhou ? `400 Bad Request`
- [x] Processamento assíncrono ? `202 Accepted`

### PATCH
- [x] Atualização bem-sucedida ? `200 OK`
- [x] Recurso não encontrado ? `404 Not Found` ?
- [x] Regra de negócio violada ? `400 Bad Request`

### DELETE
- [x] Deletado com sucesso ? `204 No Content`
- [x] Recurso não encontrado ? `404 Not Found` ?

---

## ?? Conclusão

**Correção implementada com sucesso:**

? **404 Not Found** - Quando recurso não existe  
? **400 Bad Request** - Quando validação/regra de negócio falha  
? **NotFoundException** - Exceção específica para não encontrado  
? **Result.NotFound()** - Flag explícita no Result Pattern  
? **BaseController** - Tratamento correto de IsNotFound  
? **Semântica HTTP** - Padrões RESTful aplicados  

**API agora segue status codes RESTful corretamente!** ??
