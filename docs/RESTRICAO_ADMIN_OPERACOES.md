# ? Restrição de Operações: Apenas Admin

## ?? Decisão

**Operações críticas (criar, encerrar, deletar questionários) devem ser restritas apenas para Admin!**

---

## ?? Antes vs Depois

### ? **ANTES - Qualquer Usuário Autenticado**

```csharp
[Authorize] // ? Qualquer usuário autenticado
[HttpPost]
public async Task<ActionResult<ApiResponse<QuestionarioDto>>> Criar([FromBody] CriarQuestionarioRequest request)
{
    var usuarioId = ObterUsuarioIdDoToken();
    var result = await _questionarioService.CriarQuestionarioAsync(request, usuarioId);
    return FromResult(result);
}

[Authorize] // ? Qualquer usuário autenticado
[HttpPatch("{id}/status")]
public async Task<ActionResult> Encerrar(Guid id) { ... }

[Authorize] // ? Qualquer usuário autenticado
[HttpDelete("{id}")]
public async Task<IActionResult> Deletar(Guid id) { ... }
```

**Resultado:**
- ?? Qualquer usuário pode criar questionários
- ?? Qualquer usuário pode encerrar questionários
- ?? Qualquer usuário pode deletar questionários
- ?? Sem controle de quem faz o quê

---

### ? **DEPOIS - Apenas Admin**

```csharp
[Authorize(Roles = "Admin")] // ? APENAS Admin
[HttpPost]
public async Task<ActionResult<ApiResponse<QuestionarioDto>>> Criar([FromBody] CriarQuestionarioRequest request, CancellationToken ct)
{
    var usuarioId = ObterUsuarioIdDoToken();
    var result = await _questionarioService.CriarQuestionarioAsync(request, usuarioId, ct);
    return FromResult(result);
}

[Authorize(Roles = "Admin")] // ? APENAS Admin
[HttpPatch("{id}/status")]
public async Task<ActionResult<ApiResponse<QuestionarioDto>>> Encerrar(Guid id, CancellationToken ct)
{
    var result = await _questionarioService.EncerrarQuestionarioAsync(id, ct);
    return FromResult(result);
}

[Authorize(Roles = "Admin")] // ? APENAS Admin
[HttpDelete("{id}")]
public async Task<IActionResult> Deletar(Guid id, CancellationToken ct)
{
    var result = await _questionarioService.DeletarQuestionarioAsync(id, ct);
    if (result.IsFailure)
        return FailResponseNoContent(result.Error);
    return NoContent();
}
```

**Resultado:**
- ? Apenas Admin pode criar questionários
- ? Apenas Admin pode encerrar questionários
- ? Apenas Admin pode deletar questionários
- ? Mensagem clara quando não autorizado

---

## ?? Matriz de Permissões

### QuestionarioController

| Endpoint | Método | Antes | Depois | Roles Permitidas |
|----------|--------|-------|--------|------------------|
| `POST /api/questionario` | Criar | `[Authorize]` | `[Authorize(Roles = "Admin")]` | ? Admin |
| `PATCH /api/questionario/{id}/status` | Encerrar | `[Authorize]` | `[Authorize(Roles = "Admin")]` | ? Admin |
| `DELETE /api/questionario/{id}` | Deletar | `[Authorize]` | `[Authorize(Roles = "Admin")]` | ? Admin |
| `GET /api/questionario/{id}` | Obter por ID | `[Authorize]` | `[Authorize]` | ? Todos autenticados |
| `GET /api/questionario` | Listar | `[Authorize]` | `[Authorize]` | ? Todos autenticados |
| `GET /api/questionario/{id}/resultados` | Resultados | `[Authorize(Roles = "Admin,Analista,Visualizador")]` | `[Authorize(Roles = "Admin,Analista,Visualizador")]` | ? Admin, Analista, Visualizador |

### RespostaController

| Endpoint | Método | Roles Permitidas |
|----------|--------|------------------|
| `POST /api/resposta` | Registrar | ? Todos autenticados |
| `GET /api/resposta/questionario-publico/{id}` | Obter Público | ? Sem autenticação (AllowAnonymous) |

---

## ?? Resposta HTTP: 403 Forbidden

### Cenário: Usuário sem Permissão

**Request:**
```http
POST /api/questionario
Authorization: Bearer {token-usuario-comum}
Content-Type: application/json

{
  "titulo": "Novo Questionário",
  "descricao": "Teste",
  "dataInicio": "2024-01-01",
  "dataFim": "2024-12-31",
  "perguntas": [...]
}
```

**Response: 403 Forbidden**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "traceId": "00-abc123..."
}
```

**Mensagem clara do ASP.NET Core:**
- Status: **403 Forbidden**
- Significa: "Você está autenticado, mas não tem permissão"

---

## ?? Diferença: 401 vs 403

### 401 Unauthorized

**Quando:** Token ausente ou inválido

```http
POST /api/questionario
Authorization: (vazio ou token inválido)
```

**Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

**Significado:** "Quem é você? Faça login!"

---

### 403 Forbidden

**Quando:** Token válido, mas sem role necessária

```http
POST /api/questionario
Authorization: Bearer {token-valido-mas-sem-role-admin}
```

**Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```

**Significado:** "Sei quem você é, mas você não tem permissão!"

---

## ?? Testes de Permissão

### Criar Usuário Admin (Seed)

```csharp
// DbInitializer.cs
var adminId = Guid.NewGuid();
var adminEmail = Email.Create("admin@questionario.com");
var adminSenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

var admin = new Usuario(
    id: adminId,
    nome: "Administrador",
    email: adminEmail,
    senhaHash: adminSenhaHash,
    role: UsuarioRole.Admin // ? Admin
);

context.Usuarios.Add(admin);
```

### Criar Usuário Comum (Seed)

```csharp
var usuarioId = Guid.NewGuid();
var usuarioEmail = Email.Create("usuario@questionario.com");
var usuarioSenhaHash = BCrypt.Net.BCrypt.HashPassword("User@123");

var usuario = new Usuario(
    id: usuarioId,
    nome: "Usuário Comum",
    email: usuarioEmail,
    senhaHash: usuarioSenhaHash,
    role: UsuarioRole.Usuario // ? Usuário comum
);

context.Usuarios.Add(usuario);
```

---

## ?? Casos de Uso

### Caso 1: Admin Cria Questionário ?

**1. Login como Admin:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@questionario.com",
  "senha": "Admin@123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "usuarioId": "abc-123",
    "nome": "Administrador",
    "email": "admin@questionario.com"
  }
}
```

**2. Criar Questionário:**
```http
POST /api/questionario
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: application/json

{
  "titulo": "Pesquisa de Satisfação",
  "descricao": "Avaliação do serviço",
  "dataInicio": "2024-01-01",
  "dataFim": "2024-12-31",
  "perguntas": [...]
}
```

**Response: 200 OK** ?
```json
{
  "success": true,
  "data": {
    "id": "def-456",
    "titulo": "Pesquisa de Satisfação",
    "status": "Ativo",
    ...
  }
}
```

---

### Caso 2: Usuário Comum Tenta Criar ?

**1. Login como Usuário Comum:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@questionario.com",
  "senha": "User@123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "usuarioId": "xyz-789",
    "nome": "Usuário Comum",
    "email": "usuario@questionario.com"
  }
}
```

**2. Tentar Criar Questionário:**
```http
POST /api/questionario
Authorization: Bearer eyJhbGciOiJIUzI1NiIs... (token do usuário comum)
Content-Type: application/json

{
  "titulo": "Tentativa de Criar",
  ...
}
```

**Response: 403 Forbidden** ?
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "traceId": "00-abc123..."
}
```

**Mensagem clara:** "Você não tem permissão para executar esta operação"

---

### Caso 3: Usuário Comum Pode Listar ?

**Request:**
```http
GET /api/questionario
Authorization: Bearer {token-usuario-comum}
```

**Response: 200 OK** ?
```json
{
  "success": true,
  "data": [
    {
      "id": "def-456",
      "titulo": "Pesquisa de Satisfação",
      "status": "Ativo",
      ...
    }
  ]
}
```

**Por quê?** `GET /api/questionario` tem apenas `[Authorize]` (sem Roles)

---

## ?? Justificativa: Por Que Apenas Admin?

### 1. **Controle de Qualidade**
- ? Admin garante que questionários sejam bem formulados
- ? Evita questionários duplicados ou mal feitos
- ? Padrão de qualidade mantido

### 2. **Governança**
- ? Sabe exatamente quem criou cada questionário (auditoria)
- ? Responsabilidade centralizada
- ? Processo de aprovação claro

### 3. **Segurança**
- ? Evita spam de questionários
- ? Previne uso indevido da API
- ? Controle de acesso granular

### 4. **Simplicidade para MVP**
- ? Menos complexidade de autorização
- ? Fluxo claro: Admin cria, todos respondem
- ? Fácil de entender e manter

---

## ?? Tabela Resumo de Permissões

| Operação | Admin | Analista | Visualizador | Usuário | Anônimo |
|----------|-------|----------|--------------|---------|---------|
| **Criar Questionário** | ? | ? | ? | ? | ? |
| **Encerrar Questionário** | ? | ? | ? | ? | ? |
| **Deletar Questionário** | ? | ? | ? | ? | ? |
| **Listar Questionários** | ? | ? | ? | ? | ? |
| **Obter Questionário por ID** | ? | ? | ? | ? | ? |
| **Ver Resultados** | ? | ? | ? | ? | ? |
| **Registrar Resposta** | ? | ? | ? | ? | ? |
| **Obter Questionário Público** | ? | ? | ? | ? | ? |

---

## ?? Configuração JWT (Claims)

### Token do Admin

```json
{
  "sub": "abc-123",
  "email": "admin@questionario.com",
  "name": "Administrador",
  "role": "Admin", // ? Claim importante
  "nbf": 1704067200,
  "exp": 1704153600,
  "iat": 1704067200,
  "iss": "QuestionarioOnlineApi",
  "aud": "QuestionarioOnlineClient"
}
```

### Token do Usuário Comum

```json
{
  "sub": "xyz-789",
  "email": "usuario@questionario.com",
  "name": "Usuário Comum",
  "role": "Usuario", // ? Sem role Admin
  "nbf": 1704067200,
  "exp": 1704153600,
  "iat": 1704067200,
  "iss": "QuestionarioOnlineApi",
  "aud": "QuestionarioOnlineClient"
}
```

**ASP.NET Core valida automaticamente:**
```csharp
[Authorize(Roles = "Admin")] // ? Verifica claim "role" == "Admin"
```

---

## ? Conclusão

**Apenas Admin pode criar/encerrar/deletar questionários:**

? **Segurança** - Controle de acesso granular  
? **Governança** - Responsabilidade centralizada  
? **Qualidade** - Padrão mantido  
? **Simplicidade** - Fluxo claro para MVP  
? **Mensagem clara** - 403 Forbidden automático  

**Fluxo MVP:**
1. ? **Admin** cria questionários
2. ? **Todos** autenticados listam questionários
3. ? **Todos** (até anônimos) respondem questionários
4. ? **Admin/Analista/Visualizador** veem resultados

**Perfeito para MVP com controle e simplicidade!** ??
