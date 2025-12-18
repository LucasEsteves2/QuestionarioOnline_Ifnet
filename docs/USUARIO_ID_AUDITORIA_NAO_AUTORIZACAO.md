# ? UsuarioId: Manter para Auditoria (Não para Autorização)

## ?? Questão

**Faz sentido manter `UsuarioId` em `Questionario` se não validamos propriedade?**

---

## ? **Resposta: SIM, para Auditoria!**

### Análise

```csharp
public class Questionario
{
    public Guid UsuarioId { get; private set; } // ? Ainda faz sentido?
}
```

**Antes (com validação):**
- Usado para **autorizar** operações (encerrar, deletar, etc.)
- Usado para **filtrar** questionários (`/meus`)

**Agora (sem validação):**
- ? NÃO usado para autorização
- ? NÃO usado para filtro obrigatório
- ? **SIM** usado para **auditoria**

---

## ?? Usos Legítimos (Sem Validação)

### 1. **Auditoria / Logs**

```sql
-- Quem criou cada questionário?
SELECT Titulo, UsuarioId, DataCriacao
FROM Questionarios
ORDER BY DataCriacao DESC;

-- Resultado:
Título                    | UsuarioId                              | DataCriacao
--------------------------|----------------------------------------|-------------------
Pesquisa de Satisfação   | 123e4567-e89b-12d3-a456-426614174000  | 2024-01-15 10:30
Avaliação de Curso       | 987fcdeb-51a2-43c1-8a9b-123456789abc  | 2024-01-14 09:15
```

**Para quê?**
- ? Investigar problemas ("Quem criou esse questionário?")
- ? Rastrear atividades
- ? Compliance / LGPD

### 2. **Analytics / Métricas**

```sql
-- Quantos questionários cada usuário criou?
SELECT UsuarioId, COUNT(*) as TotalQuestionarios
FROM Questionarios
GROUP BY UsuarioId;

-- Resultado:
UsuarioId                              | TotalQuestionarios
---------------------------------------|-------------------
123e4567-e89b-12d3-a456-426614174000  | 15
987fcdeb-51a2-43c1-8a9b-123456789abc  | 8
```

**Para quê?**
- ? Dashboard admin (quantos questionários por usuário)
- ? Identificar usuários mais ativos
- ? Métricas de uso

### 3. **Troubleshooting**

```csharp
// Investigar problema reportado
var questionario = await _repo.ObterPorIdAsync(id);

_logger.LogWarning(
    "Problema no questionário {QuestionarioId} criado por {UsuarioId}",
    questionario.Id,
    questionario.UsuarioId); // ? Útil para investigar

// Contatar usuário que criou para mais informações
var usuario = await _usuarioRepo.ObterPorIdAsync(questionario.UsuarioId);
await _emailService.EnviarAsync(usuario.Email, "Problema no seu questionário...");
```

### 4. **Evolução Futura (Opcional)**

```csharp
// Se no futuro precisar adicionar filtro (sem obrigar)
[HttpGet]
public async Task<ActionResult> Listar([FromQuery] Guid? usuarioId = null)
{
    if (usuarioId.HasValue)
        return await _service.ListarPorUsuarioAsync(usuarioId.Value); // Filtro OPCIONAL
    
    return await _service.ListarTodosAsync(); // Padrão: todos
}
```

---

## ? **O Que NÃO Fazemos Mais**

### Validação de Propriedade (Removida)

```csharp
// ? REMOVIDO - Não validamos mais
public void EncerrarPor(Guid usuarioId)
{
    if (UsuarioId != usuarioId)
        throw new DomainException("Não autorizado");
    // ...
}

// ? AGORA - Simples, sem validação
public void Encerrar()
{
    if (Status == StatusQuestionario.Encerrado)
        throw new DomainException("Já encerrado");
    
    Status = StatusQuestionario.Encerrado;
    DataEncerramento = DateTime.UtcNow;
}
```

### Filtro Obrigatório (Removido)

```csharp
// ? REMOVIDO - Endpoint /meus
[HttpGet("meus")]
public async Task<ActionResult> ListarMeus()
{
    var usuarioId = ObterUsuarioIdDoToken();
    return await _service.ListarPorUsuarioAsync(usuarioId);
}

// ? AGORA - Apenas 1 endpoint que lista todos
[HttpGet]
public async Task<ActionResult> Listar()
{
    return await _service.ListarTodosAsync();
}
```

---

## ?? Implementação Final

### Domain: Simplificado (Sem Validação)

```csharp
public class Questionario
{
    public Guid UsuarioId { get; private set; } // ? Apenas auditoria
    
    public static Questionario Criar(string titulo, string? descricao, DateTime dataInicio, DateTime dataFim, Guid usuarioId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(titulo, nameof(titulo));
        
        // ? REMOVIDO - Validação de usuarioId
        // if (usuarioId == Guid.Empty)
        //     throw new ArgumentException("Usuário inválido", nameof(usuarioId));
        
        var periodoColeta = PeriodoColeta.Create(dataInicio, dataFim);

        return new Questionario
        {
            Id = Guid.NewGuid(),
            Titulo = titulo,
            Descricao = descricao,
            Status = StatusQuestionario.Ativo,
            PeriodoColeta = periodoColeta,
            UsuarioId = usuarioId, // ? Guardado apenas para auditoria
            DataCriacao = DateTime.UtcNow
        };
    }
    
    // ? REMOVIDO - Métodos de validação de usuário
    // public void GarantirQueUsuarioPodeAcessar(Guid usuarioId) { ... }
    // public void EncerrarPor(Guid usuarioId) { ... }
}
```

### Database: Schema Mantido

```sql
CREATE TABLE Questionarios (
    Id uniqueidentifier PRIMARY KEY,
    Titulo nvarchar(200) NOT NULL,
    Descricao nvarchar(1000),
    Status int NOT NULL,
    DataInicio datetime2 NOT NULL,
    DataFim datetime2 NOT NULL,
    UsuarioId uniqueidentifier NOT NULL, -- ? Mantido para auditoria
    DataCriacao datetime2 NOT NULL,
    DataEncerramento datetime2,
    
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) -- ? FK mantida
);
```

**Por quê manter FK?**
- ? Integridade referencial (se deletar usuário, decidir o que fazer com questionários)
- ? Joins para relatórios
- ? Facilita queries de auditoria

---

## ?? Comparação: Antes vs Depois

| Aspecto | Antes | Depois |
|---------|-------|--------|
| **Coluna UsuarioId no DB** | ? Existe | ? Existe (mantida) |
| **Foreign Key** | ? Sim | ? Sim (mantida) |
| **Validação Empty** | ? `if (userId == Guid.Empty)` | ? Removida |
| **Usado para autorização** | ? Sim | ? Não |
| **Usado para filtro** | ? Endpoint `/meus` | ? Não |
| **Usado para auditoria** | ? Sim | ? Sim |
| **Usado para analytics** | ? Sim | ? Sim |

---

## ?? Exemplos de Queries de Auditoria

### 1. Listar com Nome do Usuário

```sql
SELECT 
    q.Id,
    q.Titulo,
    q.Status,
    q.DataCriacao,
    u.Nome as CriadoPor,
    u.Email
FROM Questionarios q
INNER JOIN Usuarios u ON q.UsuarioId = u.Id
ORDER BY q.DataCriacao DESC;
```

**Resultado:**
```
Id                                     | Titulo                  | Status | DataCriacao         | CriadoPor      | Email
---------------------------------------|-------------------------|--------|---------------------|----------------|-------------------
abc123...                              | Pesquisa Satisfação    | 1      | 2024-01-15 10:30   | João Silva     | joao@email.com
def456...                              | Avaliação Curso        | 2      | 2024-01-14 09:15   | Maria Santos   | maria@email.com
```

### 2. Dashboard: Questionários por Usuário

```csharp
// Application Service
public async Task<IEnumerable<UsuarioQuestionariosDto>> ObterDashboardAsync()
{
    var questionarios = await _repo.ObterTodosAsync();
    var usuarios = await _usuarioRepo.ObterTodosAsync();
    
    return usuarios.Select(u => new UsuarioQuestionariosDto
    {
        UsuarioId = u.Id,
        Nome = u.Nome,
        Email = u.Email,
        TotalQuestionarios = questionarios.Count(q => q.UsuarioId == u.Id),
        QuestionariosAtivos = questionarios.Count(q => q.UsuarioId == u.Id && q.Status == StatusQuestionario.Ativo)
    });
}
```

**Response:**
```json
[
  {
    "usuarioId": "123e4567-e89b-12d3-a456-426614174000",
    "nome": "João Silva",
    "email": "joao@email.com",
    "totalQuestionarios": 15,
    "questionariosAtivos": 8
  },
  {
    "usuarioId": "987fcdeb-51a2-43c1-8a9b-123456789abc",
    "nome": "Maria Santos",
    "email": "maria@email.com",
    "totalQuestionarios": 8,
    "questionariosAtivos": 3
  }
]
```

### 3. Logs Estruturados

```csharp
public async Task<Result<QuestionarioDto>> EncerrarQuestionarioAsync(Guid id, CancellationToken ct)
{
    var questionario = await _repo.ObterPorIdAsync(id);
    
    if (questionario is null)
        return Result.Failure<QuestionarioDto>("Não encontrado");
    
    try
    {
        questionario.Encerrar();
        await _repo.AtualizarAsync(questionario);
        
        // ? Log estruturado com auditoria
        _logger.LogInformation(
            "Questionário {QuestionarioId} encerrado. Criado por {UsuarioId} em {DataCriacao}",
            questionario.Id,
            questionario.UsuarioId, // ? Auditoria
            questionario.DataCriacao);
        
        return Result.Success(QuestionarioMapper.ToDto(questionario));
    }
    catch (DomainException ex)
    {
        return Result.Failure<QuestionarioDto>(ex.Message);
    }
}
```

---

## ? Decisão Final

### ? **MANTER UsuarioId**

**Motivos:**
1. ? **Auditoria** - Rastreabilidade essencial
2. ? **Analytics** - Métricas por usuário
3. ? **Troubleshooting** - Investigar problemas
4. ? **Compliance** - LGPD/regulamentações
5. ? **Custo zero** - Já existe, não custa nada manter
6. ? **Evolução futura** - Pode adicionar filtros opcionais depois

### ? **REMOVIDO**

1. ? Validação `if (usuarioId == Guid.Empty)` - Desnecessária
2. ? Método `GarantirQueUsuarioPodeAcessar(userId)` - Não valida mais
3. ? Método `EncerrarPor(userId)` - Substituído por `Encerrar()`
4. ? Endpoint `GET /meus` - Desnecessário para MVP

---

## ?? Regra de Ouro

```
??????????????????????????????????????????????????
? UsuarioId em Questionario:                    ?
?                                                ?
? ? PARA: Auditoria, logs, analytics           ?
? ? NÃO PARA: Autorização, validação           ?
?                                                ?
? Guardar quem criou ? Validar propriedade     ?
??????????????????????????????????????????????????
```

---

## ?? Benefícios de Manter

| Benefício | Descrição | Custo |
|-----------|-----------|-------|
| **Auditoria** | Saber quem criou cada questionário | Zero (já existe) |
| **Rastreabilidade** | Logs estruturados | Zero |
| **Analytics** | Dashboard de métricas | Zero |
| **Troubleshooting** | Investigar problemas | Zero |
| **Compliance** | LGPD/regulamentações | Zero |
| **Flexibilidade futura** | Adicionar filtros opcionais | Zero |

**Total: Zero custo, múltiplos benefícios!**

---

## ? Conclusão

**Manter `UsuarioId` faz total sentido!**

? **Auditoria** - Essencial para rastreabilidade  
? **Analytics** - Métricas e relatórios  
? **Sem custo** - Já existe no schema  
? **Sem complexidade** - Não valida, apenas guarda  
? **Futuro** - Pode adicionar filtros opcionais  

**Não confundir:**
- ? **Guardar UsuarioId** = Auditoria (sempre útil)
- ? **Validar UsuarioId** = Autorização (removida para MVP)

**Decisão: MANTER para auditoria!** ??
