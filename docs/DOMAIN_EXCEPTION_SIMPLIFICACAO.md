# ? Simplificação: DomainException Única

## ?? Decisão de Design

Ao invés de criar uma hierarquia de exceções específicas para cada tipo de erro de domínio, optamos por usar **uma única `DomainException`** com mensagens descritivas.

---

## ?? Antes vs Depois

### ? **ANTES - Hierarquia Complexa**

```
QuestionarioOnline.Domain/
??? Exceptions/
    ??? DomainException.cs (abstrata)
    ??? QuestionarioExceptions.cs
    ?   ??? QuestionarioJaEncerradoException
    ?   ??? QuestionarioNaoPodeReceberRespostasException
    ?   ??? PerguntaDuplicadaException
    ??? RespostaExceptions.cs
    ?   ??? RespostaInvalidaException
    ?   ??? PerguntaObrigatoriaException
    ?   ??? RespostaDuplicadaException
    ??? UsuarioExceptions.cs
        ??? UsuarioInvalidoException
        ??? UsuarioInativoException
        ??? EmailJaCadastradoException
```

**Problemas:**
- ?? **Overhead desnecessário** - 10+ classes de exceção
- ?? **Manutenção complexa** - Precisa criar nova classe para cada erro
- ?? **Pouco valor agregado** - Exceções não têm comportamento diferente

---

### ? **DEPOIS - Exceção Única**

```
QuestionarioOnline.Domain/
??? Exceptions/
    ??? DomainException.cs (concreta)
```

```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
```

**Benefícios:**
- ? **Simplicidade** - 1 classe ao invés de 10+
- ? **Flexibilidade** - Fácil adicionar novos erros (só mudar mensagem)
- ? **Manutenibilidade** - Menos código para manter
- ? **Mesmo comportamento** - Application captura `DomainException` de qualquer forma

---

## ?? Uso no Domain

### Domain agora lança exceções com mensagens descritivas:

```csharp
// Questionario.cs
public void Encerrar()
{
    if (Status == StatusQuestionario.Encerrado)
        throw new DomainException("Questionário já está encerrado");

    Status = StatusQuestionario.Encerrado;
    DataEncerramento = DateTime.UtcNow;
}

public void GarantirQuePodeReceberRespostas()
{
    if (Status != StatusQuestionario.Ativo)
        throw new DomainException("Questionário não está ativo");

    if (!PeriodoColeta.EstaAtivo())
        throw new DomainException("Período de coleta encerrado");
}

private void GarantirQueNaoEstaEncerrado()
{
    if (Status == StatusQuestionario.Encerrado)
        throw new DomainException("Não é possível realizar esta operação em um questionário encerrado");
}
```

```csharp
// Resposta.cs
public void AdicionarItem(RespostaItem item)
{
    ArgumentNullException.ThrowIfNull(item, nameof(item));

    if (_itens.Any(i => i.PerguntaId == item.PerguntaId))
        throw new DomainException("Já existe uma resposta para esta pergunta");

    _itens.Add(item);
}

public void GarantirCompletude(IEnumerable<Pergunta> perguntas)
{
    var perguntasObrigatorias = perguntas.Where(p => p.Obrigatoria).ToList();
    var perguntasRespondidas = _itens.Select(i => i.PerguntaId).ToList();

    foreach (var pergunta in perguntasObrigatorias)
    {
        if (!perguntasRespondidas.Contains(pergunta.Id))
            throw new DomainException($"A pergunta '{pergunta.Texto}' é obrigatória e não foi respondida");
    }
}
```

```csharp
// Usuario.cs
public void GarantirQueEstaAtivo()
{
    if (!Ativo)
        throw new DomainException("Usuário está inativo e não pode realizar esta operação");
}
```

```csharp
// AuthService.cs
var usuarioExistente = await _usuarioRepository.ObterPorEmailAsync(email);
if (usuarioExistente != null)
    throw new DomainException($"O email '{request.Email}' já está cadastrado no sistema");
```

---

## ?? Application não muda

```csharp
// Application Service
public async Task<Result<QuestionarioDto>> EncerrarQuestionarioAsync(...)
{
    try
    {
        questionario.Encerrar();
        await _repository.AtualizarAsync(questionario);
        return Result.Success(dto);
    }
    catch (DomainException ex) // ? Captura todas as exceções de domínio
    {
        return Result.Failure<QuestionarioDto>(ex.Message);
    }
}
```

**Não importa qual erro de domínio aconteça, o tratamento é o mesmo:**
1. Captura `DomainException`
2. Retorna `Result.Failure` com a mensagem
3. Controller traduz para HTTP response

---

## ?? Quando criar exceções específicas?

### ? **Crie exceções específicas quando:**
1. **Comportamento diferente** - Exceção precisa de lógica específica
2. **Metadados adicionais** - Exceção carrega informações extras
3. **Tratamento específico** - Application trata cada exceção de forma diferente

### Exemplo (se fosse necessário):
```csharp
public class ValidacaoException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }
    
    public ValidacaoException(Dictionary<string, string[]> errors) 
        : base("Erros de validação")
    {
        Errors = errors;
    }
}
```

---

## ?? Vantagens da Abordagem Simples

### 1. **YAGNI (You Aren't Gonna Need It)**
- Não criamos abstração até que seja realmente necessário
- Mensagens descritivas são suficientes

### 2. **KISS (Keep It Simple, Stupid)**
- 1 classe é mais simples que 10+
- Menos código = menos bugs

### 3. **Pragmatismo**
- Foco no que agrega valor
- Application trata todas as exceções de domínio igualmente

### 4. **Manutenibilidade**
```csharp
// Adicionar novo erro é trivial:
throw new DomainException("Nova regra de negócio violada");

// VS criar nova classe:
public class NovaRegraException : DomainException { ... }
```

---

## ?? Quando evoluir para hierarquia?

Se no futuro você precisar:

```csharp
// Application precisa tratar cada erro diferente
try
{
    domain.Operacao();
}
catch (PagamentoRecusadoException ex)
{
    // Envia email de cobrança
    await _emailService.EnviarCobrancaAsync(ex.TransacaoId);
    return Result.Failure("Pagamento recusado");
}
catch (EstoqueInsuficienteException ex)
{
    // Notifica compras
    await _notificacao.AlertarComprasAsync(ex.ProdutoId);
    return Result.Failure("Estoque insuficiente");
}
catch (DomainException ex)
{
    return Result.Failure(ex.Message);
}
```

**Aí sim faz sentido** criar exceções específicas!

---

## ? Conclusão

**Por ora, `DomainException` única é suficiente e recomendado:**

? Simples  
? Pragmático  
? Fácil de manter  
? Atende todos os casos de uso atuais  

**Se surgir necessidade de comportamento diferenciado, evolua a hierarquia.**

Mas até lá: **YAGNI + KISS!** ??
