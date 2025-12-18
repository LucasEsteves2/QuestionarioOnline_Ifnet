# ?? RESTful Best Practices: O Que Retornar em Cada Endpoint

## ?? Questão Central

**O que devemos retornar em cada tipo de operação REST?**

- ? POST (201 Created) ? Retornar objeto completo ou apenas ID?
- ? PUT/PATCH ? Retornar objeto atualizado ou apenas 204 No Content?
- ? GET ? Sempre retornar objeto completo?
- ? DELETE ? 204 No Content ou 200 OK com mensagem?

---

## ?? RFC 7231 - HTTP/1.1 Semantics (Padrão Oficial)

### Status Codes Recomendados

| Método | Operação | Status Sucesso | O Que Retornar |
|--------|----------|----------------|----------------|
| **POST** | Criar recurso | `201 Created` | Recurso criado + `Location` header |
| **GET** | Obter recurso(s) | `200 OK` | Recurso(s) completo(s) |
| **PUT** | Substituir recurso | `200 OK` ou `204 No Content` | Recurso atualizado OU vazio |
| **PATCH** | Atualizar parcial | `200 OK` ou `204 No Content` | Recurso atualizado OU vazio |
| **DELETE** | Deletar recurso | `204 No Content` ou `200 OK` | Vazio OU confirmação |

---

## ?? Best Practices por Operação

### 1?? **POST - Criar Recurso (201 Created)**

#### ? **Melhor Prática (Consenso da Indústria)**

```http
POST /api/questionario
Content-Type: application/json

{
  "titulo": "Pesquisa 2024",
  "descricao": "...",
  ...
}
```

**Response:**
```http
HTTP/1.1 201 Created
Location: /api/questionario/abc-123-def-456
Content-Type: application/json

{
  "id": "abc-123-def-456",
  "titulo": "Pesquisa 2024",
  "status": "Ativo",
  "dataInicio": "2024-01-01T00:00:00Z",
  "dataFim": "2024-12-31T23:59:59Z",
  "perguntas": [...]
}
```

**Por quê retornar o objeto completo?**
1. ? **Evita roundtrip** - Cliente não precisa fazer `GET` imediatamente após `POST`
2. ? **Dados calculados** - Campos gerados pelo servidor (ID, timestamps, valores default)
3. ? **Confirmação** - Cliente vê exatamente o que foi criado
4. ? **Location header** - Cliente sabe onde buscar o recurso depois

**Referências:**
- **Microsoft REST API Guidelines:** "POST should return the created resource"
- **GitHub API:** Retorna objeto completo
- **Stripe API:** Retorna objeto completo
- **Google Cloud API:** Retorna objeto completo

---

#### ? **Anti-Pattern: Retornar Apenas ID**

```http
HTTP/1.1 201 Created
Content-Type: application/json

{
  "id": "abc-123-def-456"
}
```

**Problemas:**
- ? Cliente precisa fazer `GET /api/questionario/abc-123-def-456` imediatamente
- ? 2 requests ao invés de 1 (overhead de rede)
- ? Cliente não vê dados calculados/default imediatamente
- ? Experiência ruim para frontend

---

#### ?? **Exceção: Operações Assíncronas**

```http
POST /api/resposta
Content-Type: application/json

{
  "questionarioId": "...",
  "respostas": [...]
}
```

**Response:**
```http
HTTP/1.1 202 Accepted
Location: /api/resposta/status/xyz-789

{
  "id": "xyz-789",
  "status": "Processing",
  "message": "Resposta será processada em breve"
}
```

**Por quê 202 Accepted?**
- ? Processamento assíncrono (Azure Queue)
- ? Recurso ainda não foi totalmente criado
- ? Cliente pode consultar status depois

---

### 2?? **GET - Obter Recurso (200 OK)**

#### ? **Melhor Prática**

```http
GET /api/questionario/abc-123
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": "abc-123",
  "titulo": "Pesquisa 2024",
  "status": "Ativo",
  "perguntas": [...],
  ...
}
```

**Regras:**
- ? Sempre retornar o recurso **completo**
- ? Incluir relacionamentos (perguntas, opções)
- ? Timestamps (dataInicio, dataFim, dataCriacao)

---

#### ?? **GET de Lista (200 OK)**

```http
GET /api/questionario
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

[
  {
    "id": "abc-123",
    "titulo": "Pesquisa 2024",
    "status": "Ativo",
    "totalPerguntas": 5
  },
  ...
]
```

**Regras:**
- ? Retornar **representação simplificada** (sem nested objects)
- ? Incluir campos essenciais (id, titulo, status)
- ? Omitir relacionamentos profundos (perguntas completas)
- ? Considerar paginação (se muitos resultados)

---

### 3?? **PUT/PATCH - Atualizar Recurso**

#### ? **Opção 1: Retornar Objeto Atualizado (200 OK)**

```http
PATCH /api/questionario/abc-123/status
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": "abc-123",
  "titulo": "Pesquisa 2024",
  "status": "Encerrado",    ? Atualizado
  "dataEncerramento": "2024-01-15T10:30:00Z"   ? Novo campo
}
```

**Quando usar:**
- ? Quando servidor **calcula valores** (dataEncerramento, updatedAt)
- ? Quando cliente precisa **ver o resultado** imediatamente
- ? Quando atualização **gera novos dados**

---

#### ? **Opção 2: Sem Corpo (204 No Content)**

```http
PATCH /api/questionario/abc-123/status
```

**Response:**
```http
HTTP/1.1 204 No Content
```

**Quando usar:**
- ? Quando cliente **já sabe o resultado** (nada calculado pelo servidor)
- ? Quando atualização é **simples** (apenas muda status)
- ? Quando cliente **não precisa** do objeto atualizado imediatamente

---

**?? Comparação:**

| Abordagem | Vantagem | Desvantagem |
|-----------|----------|-------------|
| **200 OK + Body** | Cliente vê resultado; Evita GET extra | Mais payload de rede |
| **204 No Content** | Menos payload; Mais rápido | Cliente pode precisar fazer GET |

**Recomendação:** Use **200 OK + Body** se servidor calcula valores; caso contrário **204 No Content**.

---

### 4?? **DELETE - Deletar Recurso**

#### ? **Opção 1: Sem Corpo (204 No Content) - Recomendado**

```http
DELETE /api/questionario/abc-123
```

**Response:**
```http
HTTP/1.1 204 No Content
```

**Por quê:**
- ? Recurso foi deletado; não há nada para retornar
- ? Padrão mais comum na indústria
- ? Menos payload

---

#### ?? **Opção 2: Com Confirmação (200 OK)**

```http
DELETE /api/questionario/abc-123
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "message": "Questionário deletado com sucesso",
  "deletedAt": "2024-01-15T10:30:00Z"
}
```

**Quando usar:**
- ?? Quando cliente precisa de **confirmação explícita**
- ?? Quando operação é **soft delete** (recurso marcado como deletado, mas ainda existe)
- ?? Quando há **metadados úteis** (timestamp, usuário que deletou)

**Recomendação:** Use **204 No Content** (mais RESTful).

---

## ?? Análise do Código Atual

### ? **Problema 1: POST Retornando 200 OK**

```csharp
// HOJE (ERRADO)
[HttpPost]
public async Task<ActionResult<ApiResponse<QuestionarioResponse>>> Criar(...)
{
    var response = QuestionarioResponse.From(result.Value);
    return OkResponse(response); // ? 200 OK em POST
}
```

**Problemas:**
- ? Deveria ser `201 Created`
- ? Falta `Location` header

---

### ? **Solução: POST com 201 Created + Location**

```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<QuestionarioResponse>>> Criar(...)
{
    var response = QuestionarioResponse.From(result.Value);
    return CreatedAtAction(
        nameof(ObterPorId),              // Action que retorna o recurso
        new { id = response.Id },        // Route values
        ApiResponse<QuestionarioResponse>.Success(response, statusCode: 201)
    );
}
```

**Response HTTP:**
```http
HTTP/1.1 201 Created
Location: /api/questionario/abc-123-def-456
Content-Type: application/json

{
  "success": true,
  "data": {
    "id": "abc-123-def-456",
    "titulo": "Pesquisa 2024",
    ...
  }
}
```

---

### ? **Problema 2: PATCH Retornando Objeto Completo (Desnecessário?)**

```csharp
// HOJE
[HttpPatch("{id}/status")]
public async Task<ActionResult<ApiResponse<QuestionarioResponse>>> Encerrar(...)
{
    var response = QuestionarioResponse.From(result.Value);
    return OkResponse(response); // ? OK, mas poderia ser 204 No Content
}
```

**Análise:**
- ? **Retornar objeto completo:** Bom porque `dataEncerramento` é calculada pelo servidor
- ?? **Alternativa:** Retornar apenas `{ "status": "Encerrado", "dataEncerramento": "..." }`

**Recomendação:** Manter como está (objeto completo) porque servidor calcula `dataEncerramento`.

---

### ? **Problema 3: DELETE com 204 No Content (Correto!)**

```csharp
// HOJE (CORRETO)
[HttpDelete("{id}")]
public async Task<IActionResult> Deletar(...)
{
    if (result.IsFailure)
        return FailResponseNoContent(result.Error);

    return NoContent(); // ? 204 No Content correto
}
```

**Análise:** ? Perfeito! Segue best practices.

---

## ?? Proposta de Melhoria

### **Criar Helper no BaseController**

```csharp
// BaseController.cs
protected ActionResult<ApiResponse<T>> CreatedResponse<T>(
    string actionName,
    object routeValues,
    T data)
{
    var response = ApiResponse<T>.Success(data, statusCode: 201);
    return CreatedAtAction(actionName, routeValues, response);
}
```

**Uso:**
```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<QuestionarioResponse>>> Criar(...)
{
    var response = QuestionarioResponse.From(result.Value);
    return CreatedResponse(nameof(ObterPorId), new { id = response.Id }, response);
}
```

---

## ?? Checklist de Best Practices

### ? **POST (Criar)**
- [x] Retorna `201 Created`
- [x] Retorna objeto completo
- [x] Inclui `Location` header
- [ ] **Falta implementar:** CreatedAtAction

### ? **GET (Obter)**
- [x] Retorna `200 OK`
- [x] Retorna objeto completo
- [x] Lista retorna representação simplificada

### ? **PATCH (Atualizar)**
- [x] Retorna `200 OK` + objeto atualizado
- [x] Inclui dados calculados pelo servidor (dataEncerramento)
- [ ] **Considerar:** 204 No Content se não houver dados calculados

### ? **DELETE (Deletar)**
- [x] Retorna `204 No Content`
- [x] Sem corpo de resposta

---

## ?? Referências da Indústria

### **Microsoft REST API Guidelines**
> "For POST requests that create a resource, the response should be 201 Created with a Location header and the created resource in the response body."

### **GitHub API**
```http
POST /repos/{owner}/{repo}/issues
201 Created
Location: /repos/octocat/Hello-World/issues/1347

{
  "id": 1347,
  "title": "Found a bug",
  "state": "open",
  ...
}
```

### **Stripe API**
```http
POST /v1/customers
201 Created

{
  "id": "cus_123",
  "email": "customer@example.com",
  "created": 1483565364,
  ...
}
```

### **Google Cloud API**
> "Create operations should return the created resource. This allows clients to obtain values computed by the service."

---

## ? Recomendações Finais

### **1. POST - Manter Objeto Completo ?**

**Razão:** Cliente evita GET extra; vê dados calculados (ID, timestamps, defaults).

**Mudança necessária:**
```csharp
// DE:
return OkResponse(response); // 200 OK

// PARA:
return CreatedResponse(nameof(ObterPorId), new { id = response.Id }, response); // 201 Created + Location
```

---

### **2. GET - Manter Como Está ?**

**Razão:** Já segue best practices.

- GET por ID ? Objeto completo ?
- GET lista ? Representação simplificada ?

---

### **3. PATCH - Manter Objeto Completo ?**

**Razão:** Servidor calcula `dataEncerramento`, cliente precisa ver.

**Alternativa futura (otimização):**
```csharp
// Retornar apenas campos alterados
{
  "status": "Encerrado",
  "dataEncerramento": "2024-01-15T10:30:00Z"
}
```

---

### **4. DELETE - Manter 204 No Content ?**

**Razão:** Já segue best practices RESTful.

---

## ?? Comparação: Antes vs Depois

| Operação | Antes | Depois | Melhoria |
|----------|-------|--------|----------|
| **POST Criar** | 200 OK + objeto | 201 Created + Location + objeto | ? Padrão RESTful |
| **GET Obter** | 200 OK + objeto | 200 OK + objeto | ? Já correto |
| **GET Lista** | 200 OK + lista simplificada | 200 OK + lista simplificada | ? Já correto |
| **PATCH Atualizar** | 200 OK + objeto | 200 OK + objeto | ? Já correto |
| **DELETE Deletar** | 204 No Content | 204 No Content | ? Já correto |

---

## ?? Conclusão

### **Seu Código Já Está 90% Correto!**

? **Está bem:**
- Retorna objetos completos em POST
- GET retorna representações adequadas
- DELETE usa 204 No Content
- PATCH retorna dados calculados

? **Precisa ajustar:**
- POST deve retornar `201 Created` ao invés de `200 OK`
- POST deve incluir `Location` header

---

### **Mudança Necessária: Apenas Status Code**

```csharp
// DE:
return OkResponse(response); // 200 OK

// PARA:
return CreatedAtAction(
    nameof(ObterPorId),
    new { id = response.Id },
    ApiResponse<QuestionarioResponse>.Success(response, statusCode: 201)
); // 201 Created + Location
```

**Impacto:** Minimal (apenas melhora semântica HTTP).

---

## ?? TL;DR (Resumo Executivo)

**Pergunta:** Retornar objeto completo ou apenas ID no 201 Created?

**Resposta:** **Objeto completo!**

**Por quê?**
1. ? **Evita roundtrip** - Cliente não precisa fazer GET imediatamente
2. ? **Dados calculados** - ID, timestamps, valores default
3. ? **Padrão da indústria** - Microsoft, GitHub, Stripe, Google
4. ? **Melhor UX** - Cliente vê exatamente o que foi criado

**Mudança necessária:**
- POST deve retornar `201 Created` + `Location` header (hoje retorna `200 OK`)

**Resto está perfeito!** ?
