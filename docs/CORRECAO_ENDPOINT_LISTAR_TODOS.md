# ? Correção: Endpoint de Listagem de Questionários

## ?? Problema Identificado

**Endpoint `GET /api/questionario` estava filtrando por usuário quando deveria listar TODOS!**

### ? **ANTES - Lógica Errada**

```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<IEnumerable<QuestionarioListaDto>>>> Listar(CancellationToken cancellationToken)
{
    var usuarioId = ObterUsuarioIdDoToken(); // ? Filtra por usuário
    var questionarios = await _questionarioService.ListarQuestionariosPorUsuarioAsync(usuarioId, cancellationToken);
    return OkResponse(questionarios);
}
```

**Resultado:**
- ?? Cada usuário só via **seus próprios** questionários
- ?? Impossível ver **todos** os questionários do sistema
- ?? Autenticação sendo usada para **filtrar** ao invés de **autorizar**

**Confusão Conceitual:**
- ? **Autenticação** ? **Filtro de dados**
- ? **Autenticação** = Garantir que usuário tem **acesso** ao endpoint
- ? **Filtro de dados** = Parâmetro explícito ou endpoint separado

---

## ? **DEPOIS - Lógica Correta**

### 2 Endpoints: Todos vs Meus

```csharp
// ? Lista TODOS os questionários do sistema
[HttpGet]
public async Task<ActionResult<ApiResponse<IEnumerable<QuestionarioListaDto>>>> Listar(CancellationToken cancellationToken)
{
    var questionarios = await _questionarioService.ListarTodosQuestionariosAsync(cancellationToken);
    return OkResponse(questionarios);
}

// ? Lista apenas questionários DO USUÁRIO autenticado
[HttpGet("meus")]
public async Task<ActionResult<ApiResponse<IEnumerable<QuestionarioListaDto>>>> ListarMeus(CancellationToken cancellationToken)
{
    var usuarioId = ObterUsuarioIdDoToken();
    var questionarios = await _questionarioService.ListarQuestionariosPorUsuarioAsync(usuarioId, cancellationToken);
    return OkResponse(questionarios);
}
```

**Endpoints:**
- ? `GET /api/questionario` - Lista **TODOS** (requer autenticação)
- ? `GET /api/questionario/meus` - Lista **MEUS** (filtrado por usuário autenticado)

---

## ?? Comparação

| Aspecto | Antes | Depois |
|---------|-------|--------|
| **GET /api/questionario** | Lista apenas do usuário | Lista TODOS ? |
| **Filtro por usuário** | Automático (implícito) | Endpoint explícito `/meus` ? |
| **Clareza** | Confuso | Óbvio ? |
| **Flexibilidade** | Apenas 1 opção | 2 opções ? |

---

## ?? Mudanças Implementadas

### 1. **Interface IQuestionarioService**

```csharp
public interface IQuestionarioService
{
    // ? NOVO - Lista TODOS
    Task<IEnumerable<QuestionarioListaDto>> ListarTodosQuestionariosAsync(CancellationToken cancellationToken = default);
    
    // ? Mantido - Lista por usuário específico
    Task<IEnumerable<QuestionarioListaDto>> ListarQuestionariosPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
```

### 2. **Interface IQuestionarioRepository**

```csharp
public interface IQuestionarioRepository
{
    // ? NOVO - Busca TODOS
    Task<IEnumerable<Questionario>> ObterTodosAsync(CancellationToken cancellationToken = default);
    
    // ? Mantido - Busca por usuário
    Task<IEnumerable<Questionario>> ObterTodosPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
```

### 3. **QuestionarioRepository**

```csharp
public async Task<IEnumerable<Questionario>> ObterTodosAsync(CancellationToken cancellationToken = default)
{
    return await _context.Questionarios
        .AsNoTracking()
        .Include(q => q.Perguntas)
        .OrderByDescending(q => q.DataCriacao)
        .ToListAsync(cancellationToken);
}
```

### 4. **QuestionarioService**

```csharp
public async Task<IEnumerable<QuestionarioListaDto>> ListarTodosQuestionariosAsync(CancellationToken cancellationToken = default)
{
    var questionarios = await _questionarioRepository.ObterTodosAsync(cancellationToken);
    return questionarios.Select(QuestionarioMapper.ToListaDto);
}
```

### 5. **QuestionarioController**

```csharp
// ? Lista TODOS
[HttpGet]
public async Task<ActionResult<ApiResponse<IEnumerable<QuestionarioListaDto>>>> Listar(CancellationToken cancellationToken)
{
    var questionarios = await _questionarioService.ListarTodosQuestionariosAsync(cancellationToken);
    return OkResponse(questionarios);
}

// ? NOVO - Lista apenas MEUS
[HttpGet("meus")]
public async Task<ActionResult<ApiResponse<IEnumerable<QuestionarioListaDto>>>> ListarMeus(CancellationToken cancellationToken)
{
    var usuarioId = ObterUsuarioIdDoToken();
    var questionarios = await _questionarioService.ListarQuestionariosPorUsuarioAsync(usuarioId, cancellationToken);
    return OkResponse(questionarios);
}
```

---

## ?? Conceito: Autenticação vs Filtro

### ? **Autenticação**

**Propósito:** Garantir que usuário tem **permissão** para acessar recurso

```csharp
[Authorize] // ? Garante que usuário está autenticado
[HttpGet]
public async Task<ActionResult> Listar()
{
    // ? TODOS os usuários autenticados podem listar questionários
    // ? Autenticação = Controle de ACESSO
}
```

### ? **Filtro de Dados**

**Propósito:** Retornar **subconjunto** de dados baseado em critério

```csharp
[Authorize]
[HttpGet("meus")] // ? URL deixa claro que é filtrado
public async Task<ActionResult> ListarMeus()
{
    var usuarioId = ObterUsuarioIdDoToken();
    // ? Filtro explícito por usuário
    // ? Filtro = Critério de BUSCA
}
```

---

## ?? Casos de Uso

### Caso 1: Ver Todos os Questionários (Dashboard Admin)

```http
GET /api/questionario
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid-1",
      "titulo": "Questionário de João",
      "status": "Ativo",
      "dataInicio": "2024-01-01",
      "dataFim": "2024-12-31",
      "totalPerguntas": 10
    },
    {
      "id": "uuid-2",
      "titulo": "Questionário de Maria",
      "status": "Encerrado",
      "dataInicio": "2023-01-01",
      "dataFim": "2023-12-31",
      "totalPerguntas": 5
    }
  ]
}
```

### Caso 2: Ver Apenas Meus Questionários

```http
GET /api/questionario/meus
Authorization: Bearer {token-do-joao}
```

**Resposta:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid-1",
      "titulo": "Questionário de João",
      "status": "Ativo",
      "dataInicio": "2024-01-01",
      "dataFim": "2024-12-31",
      "totalPerguntas": 10
    }
  ]
}
```

---

## ?? Autorização vs Autenticação

### Autenticação (WHO)

**Quem você é?**

```csharp
[Authorize] // ? Precisa estar logado
public async Task<ActionResult> Listar()
{
    // Qualquer usuário autenticado pode acessar
}
```

### Autorização (WHAT)

**O que você pode fazer?**

```csharp
[Authorize(Roles = "Admin")] // ? Precisa ser Admin
public async Task<ActionResult> DeletarTodos()
{
    // Apenas Admin pode deletar todos
}
```

**Nossa correção:**
- ? `GET /api/questionario` - Autenticação (qualquer usuário logado)
- ? `GET /api/questionario/{id}/resultados` - Autorização (apenas Analista/Admin)

---

## ?? RESTful Best Practices

### ? **URLs Semânticas**

```
GET /api/questionario          ? Lista TODOS os recursos
GET /api/questionario/meus     ? Lista MEUS recursos (filtro explícito)
GET /api/questionario/{id}     ? Busca recurso específico
```

### ? **Filtros Explícitos**

```csharp
// ? ERRADO - Filtro implícito (confuso)
[HttpGet]
public async Task<ActionResult> Listar()
{
    var userId = GetUserId(); // Filtro escondido
    return _service.ListByUser(userId);
}

// ? CORRETO - Filtro explícito (claro)
[HttpGet("meus")]
public async Task<ActionResult> ListarMeus()
{
    var userId = GetUserId();
    return _service.ListByUser(userId);
}
```

---

## ?? Resultado Final

### Endpoints Disponíveis

| Método | URL | Descrição | Autenticação |
|--------|-----|-----------|--------------|
| GET | `/api/questionario` | Lista **TODOS** os questionários | ? Requerida |
| GET | `/api/questionario/meus` | Lista questionários **DO USUÁRIO** | ? Requerida |
| GET | `/api/questionario/{id}` | Busca questionário específico | ? Requerida (+ validação de propriedade) |
| GET | `/api/questionario/{id}/resultados` | Resultados do questionário | ? Requerida (Roles: Admin, Analista) |
| POST | `/api/questionario` | Cria novo questionário | ? Requerida |
| PATCH | `/api/questionario/{id}/status` | Encerra questionário | ? Requerida (+ validação de propriedade) |
| DELETE | `/api/questionario/{id}` | Deleta questionário | ? Requerida (+ validação de propriedade) |

---

## ? Conclusão

**Correção aplicada com sucesso:**

? **GET /api/questionario** - Lista **TODOS** (autenticado)  
? **GET /api/questionario/meus** - Lista **MEUS** (filtrado)  
? **Separação clara** - Autenticação ? Filtro  
? **URLs semânticas** - `/meus` deixa claro o filtro  
? **RESTful** - Segue boas práticas  
? **Flexibilidade** - 2 opções disponíveis  

**Agora a API está correta e clara!** ??
