# ? Correção DDD: Autorização no Domain

## ?? Problema Identificado

**Regra de negócio vazou para Application!**

### ? **ANTES - Violação de DDD**

```csharp
// Application Service validando autorização
public async Task<Result<QuestionarioDto>> EncerrarQuestionarioAsync(
    Guid questionarioId,
    Guid usuarioId)
{
    var questionario = await _repo.ObterPorIdAsync(questionarioId);
    
    // ? Application fazendo validação de negócio!
    if (!UsuarioTemPermissao(questionario, usuarioId))
        return Result.Failure("Usuário não autorizado");

    questionario.Encerrar(); // Domain só encerra, não valida autorização
    await _repo.AtualizarAsync(questionario);
    
    return Result.Success(...);
}

// Método privado na Application
private static bool UsuarioTemPermissao(Questionario q, Guid userId) 
    => q.UsuarioId == userId;
```

**Problemas:**
- ?? **Regra de negócio na Application** - "Apenas o dono pode encerrar"
- ?? **Domain não protege invariante** - `Encerrar()` não valida quem está encerrando
- ?? **Duplicação** - Validação repetida em 4 lugares (Encerrar, Deletar, ObterPorId, ObterResultados)
- ?? **Domain Anemic** - Entidade burra, sem comportamento

---

## ? **DEPOIS - DDD Forte**

### Domain Protege Invariante

```csharp
// Questionario.cs (Domain)
public class Questionario
{
    public void EncerrarPor(Guid usuarioId)
    {
        GarantirQueUsuarioPodeAcessar(usuarioId); // ? Valida antes de executar
        
        if (Status == StatusQuestionario.Encerrado)
            throw new DomainException("Questionário já está encerrado");

        Status = StatusQuestionario.Encerrado;
        DataEncerramento = DateTime.UtcNow;
    }

    public void GarantirQueUsuarioPodeAcessar(Guid usuarioId)
    {
        if (UsuarioId != usuarioId)
            throw new DomainException("Usuário não autorizado a acessar este questionário");
    }
}
```

### Application Apenas Orquestra

```csharp
// QuestionarioService.cs (Application)
public async Task<Result<QuestionarioDto>> EncerrarQuestionarioAsync(
    Guid questionarioId,
    Guid usuarioId)
{
    var questionario = await _repo.ObterPorIdAsync(questionarioId);
    
    if (questionario is null)
        return Result.Failure("Questionário não encontrado");

    try
    {
        // ? Domain protege invariante
        questionario.EncerrarPor(usuarioId);
        await _repo.AtualizarAsync(questionario);
        return Result.Success(QuestionarioMapper.ToDto(questionario));
    }
    catch (DomainException ex)
    {
        return Result.Failure(ex.Message); // ? Traduz exceção para Result
    }
}
```

**Benefícios:**
- ? **Regra de negócio no Domain** - "Apenas o dono pode encerrar"
- ? **Domain protege invariante** - Valida autorização antes de encerrar
- ? **Sem duplicação** - Validação centralizada no Domain
- ? **Ubiquitous Language** - `EncerrarPor(usuarioId)` é expressivo

---

## ?? Comparação Completa

### Métodos Refatorados

| Método | Antes (Application) | Depois (Domain) |
|--------|---------------------|-----------------|
| `EncerrarQuestionarioAsync` | `if (!UsuarioTemPermissao(...))` | `questionario.EncerrarPor(usuarioId)` ? |
| `DeletarQuestionarioAsync` | `if (!UsuarioTemPermissao(...))` | `questionario.GarantirQueUsuarioPodeAcessar(...)` ? |
| `ObterQuestionarioPorIdAsync` | `if (!UsuarioTemPermissao(...))` | `questionario.GarantirQueUsuarioPodeAcessar(...)` ? |
| `ObterResultadosAsync` | `if (!UsuarioTemPermissao(...))` | `questionario.GarantirQueUsuarioPodeAcessar(...)` ? |

---

## ?? Mudanças no Domain

### 1. **Novo Método: `EncerrarPor(Guid usuarioId)`**

```csharp
// ANTES ?
public void Encerrar()
{
    if (Status == StatusQuestionario.Encerrado)
        throw new DomainException("Já encerrado");
    
    Status = StatusQuestionario.Encerrado;
    DataEncerramento = DateTime.UtcNow;
}

// DEPOIS ?
public void EncerrarPor(Guid usuarioId)
{
    GarantirQueUsuarioPodeAcessar(usuarioId); // Valida autorização
    
    if (Status == StatusQuestionario.Encerrado)
        throw new DomainException("Já encerrado");
    
    Status = StatusQuestionario.Encerrado;
    DataEncerramento = DateTime.UtcNow;
}
```

**Mudança:**
- Método agora recebe `usuarioId` e valida autorização
- Nome expressivo: `EncerrarPor` indica quem está encerrando

### 2. **Novo Método: `GarantirQueUsuarioPodeAcessar(Guid usuarioId)`**

```csharp
public void GarantirQueUsuarioPodeAcessar(Guid usuarioId)
{
    if (UsuarioId != usuarioId)
        throw new DomainException("Usuário não autorizado a acessar este questionário");
}
```

**Uso:**
- Reutilizado em: Deletar, ObterPorId, ObterResultados
- Centraliza regra de autorização
- Nome expressivo seguindo Ubiquitous Language

---

## ?? Mudanças na Application

### 1. **EncerrarQuestionarioAsync**

```csharp
// ANTES ?
public async Task<Result<QuestionarioDto>> EncerrarQuestionarioAsync(...)
{
    var questionario = await _repo.ObterPorIdAsync(...);
    
    if (!UsuarioTemPermissao(questionario, usuarioId)) // Application valida
        return Result.Failure("Não autorizado");
    
    questionario.Encerrar(); // Domain não valida
    await _repo.AtualizarAsync(questionario);
    return Result.Success(...);
}

// DEPOIS ?
public async Task<Result<QuestionarioDto>> EncerrarQuestionarioAsync(...)
{
    var questionario = await _repo.ObterPorIdAsync(...);
    
    if (questionario is null)
        return Result.Failure("Não encontrado");
    
    try
    {
        questionario.EncerrarPor(usuarioId); // Domain valida + encerra
        await _repo.AtualizarAsync(questionario);
        return Result.Success(...);
    }
    catch (DomainException ex)
    {
        return Result.Failure(ex.Message);
    }
}
```

### 2. **DeletarQuestionarioAsync**

```csharp
// ANTES ?
if (!UsuarioTemPermissao(questionario, usuarioId))
    return Result.Failure("Não autorizado");

if (await QuestionarioTemRespostas(...))
    return Result.Failure("Tem respostas");

await _repo.DeletarAsync(questionario);

// DEPOIS ?
try
{
    questionario.GarantirQueUsuarioPodeAcessar(usuarioId);
    
    if (await QuestionarioTemRespostas(...))
        throw new DomainException("Tem respostas");
    
    await _repo.DeletarAsync(questionario);
    return Result.Success();
}
catch (DomainException ex)
{
    return Result.Failure(ex.Message);
}
```

### 3. **ObterQuestionarioPorIdAsync**

```csharp
// ANTES ?
if (questionario is null || !UsuarioTemPermissao(questionario, usuarioId))
    return null;

return QuestionarioMapper.ToDto(questionario);

// DEPOIS ?
if (questionario is null)
    return null;

try
{
    questionario.GarantirQueUsuarioPodeAcessar(usuarioId);
    return QuestionarioMapper.ToDto(questionario);
}
catch (DomainException)
{
    return null;
}
```

### 4. **ObterResultadosAsync**

```csharp
// ANTES ?
if (!UsuarioTemPermissao(questionario, usuarioId))
    return Result.Failure("Não autorizado");

var respostas = await _repo.ObterRespostasAsync(...);
var resultado = CalcularResultados(...);
return Result.Success(resultado);

// DEPOIS ?
try
{
    questionario.GarantirQueUsuarioPodeAcessar(usuarioId);
    var respostas = await _repo.ObterRespostasAsync(...);
    var resultado = CalcularResultados(...);
    return Result.Success(resultado);
}
catch (DomainException ex)
{
    return Result.Failure(ex.Message);
}
```

---

## ?? Estatísticas

| Métrica | Antes | Depois | Melhoria |
|---------|-------|--------|----------|
| **Validações de autorização na Application** | 4 | 0 | ? -100% |
| **Métodos privados de validação no Service** | 1 (`UsuarioTemPermissao`) | 0 | ? Eliminado |
| **Métodos no Domain que protegem invariantes** | 1 | 3 | ? +200% |
| **Duplicação de regra** | Sim (4x) | Não | ? DRY |

---

## ?? Princípios DDD Aplicados

### ? **1. Domain Protege Invariantes**

**Invariante:** "Apenas o proprietário pode encerrar/deletar/acessar o questionário"

**Como Domain protege:**
```csharp
public void EncerrarPor(Guid usuarioId)
{
    GarantirQueUsuarioPodeAcessar(usuarioId); // ? Valida ANTES de executar
    // ...
}
```

### ? **2. Ubiquitous Language**

**Métodos expressivos que refletem linguagem do negócio:**
- `EncerrarPor(usuarioId)` - Quem está encerrando?
- `GarantirQueUsuarioPodeAcessar(usuarioId)` - Explícito e expressivo
- `GarantirQuePodeReceberRespostas()` - Nome claro da intenção

### ? **3. Tell, Don't Ask**

```csharp
// ANTES (Ask) ?
if (questionario.UsuarioId != usuarioId) // Pergunta estado
    return Failure();

questionario.Encerrar(); // Comanda sem contexto

// DEPOIS (Tell) ?
questionario.EncerrarPor(usuarioId); // Comanda com contexto
// Domain decide se permite ou não
```

### ? **4. Application Não Tem Regras de Negócio**

```csharp
// Application APENAS:
// 1. Busca entidade
// 2. Chama Domain (que valida)
// 3. Persiste se sucesso
// 4. Traduz DomainException ? Result
```

---

## ?? Fluxo Completo

### Encerrar Questionário

```
???????????????????????????????????????????????????????????????
? 1. Controller recebe request                                ?
?    POST /api/questionario/{id}/encerrar                     ?
???????????????????????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????????????????????
? 2. Application Service (QuestionarioService)                ?
?    - Busca questionário do repository                       ?
?    - Chama: questionario.EncerrarPor(usuarioId)            ?
???????????????????????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????????????????????
? 3. Domain (Questionario.EncerrarPor)                        ?
?    - GarantirQueUsuarioPodeAcessar(usuarioId)              ?
?      ?? if (UsuarioId != usuarioId) throw Exception        ?
?    - if (Status == Encerrado) throw Exception              ?
?    - Status = Encerrado                                     ?
?    - DataEncerramento = DateTime.UtcNow                     ?
???????????????????????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????????????????????
? 4. Application persiste + traduz resultado                  ?
?    - await _repo.AtualizarAsync(questionario)              ?
?    - return Result.Success(dto)                             ?
?    OU catch (DomainException) ? Result.Failure             ?
???????????????????????????????????????????????????????????????
```

---

## ? Resultado Final

### Domain Protege TODOS os Invariantes

```csharp
public class Questionario
{
    // ? Protege: Apenas dono pode encerrar
    public void EncerrarPor(Guid usuarioId) { ... }
    
    // ? Protege: Apenas dono pode acessar
    public void GarantirQueUsuarioPodeAcessar(Guid usuarioId) { ... }
    
    // ? Protege: Só recebe respostas se ativo e dentro do período
    public void GarantirQuePodeReceberRespostas() { ... }
    
    // ? Protege: Não modifica se encerrado
    private void GarantirQueNaoEstaEncerrado() { ... }
}
```

### Application Limpa

```csharp
public class QuestionarioService
{
    // ? Apenas orquestra
    // ? Busca entidade
    // ? Chama Domain (que valida)
    // ? Persiste
    // ? Traduz exceção ? Result
    
    // ? NÃO tem regras de negócio
    // ? NÃO valida autorização
    // ? NÃO valida estado
}
```

---

## ?? Conclusão

**Regra de negócio agora está 100% no Domain:**

? Domain protege invariantes  
? Application apenas orquestra  
? Ubiquitous Language aplicado  
? Tell, Don't Ask  
? Sem duplicação (DRY)  
? DDD Forte implementado  

**Projeto agora segue DDD corretamente!** ??
