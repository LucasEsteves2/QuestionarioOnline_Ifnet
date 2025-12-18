# ? Isolamento de Mapeamento: QuestionarioMapper

## ?? Objetivo

Separar **lógica de mapeamento** (Entity ? DTO) da **lógica de negócio** (Service), aplicando **Single Responsibility Principle**.

---

## ?? Antes vs Depois

### ? **ANTES - Mapeamento no Service**

```csharp
public class QuestionarioService
{
    // Métodos de negócio
    public async Task<Result<QuestionarioDto>> CriarQuestionarioAsync(...) { }
    
    // Métodos de mapeamento misturados
    private static QuestionarioDto MapearParaDto(Questionario q) => new(...);
    private static QuestionarioPublicoDto MapearParaPublicoDto(Questionario q) => new(...);
    private static QuestionarioListaDto MapearParaListaDto(Questionario q) => new(...);
}
```

**Problemas:**
- ?? Mapeamento misturado com lógica de negócio
- ?? Service com responsabilidades demais
- ?? Difícil reutilizar mapeamentos

---

### ? **DEPOIS - Mapper Isolado**

```csharp
// QuestionarioMapper.cs - Classe focada em mapeamento
internal static class QuestionarioMapper
{
    public static QuestionarioDto ToDto(Questionario q) => new(...);
    public static QuestionarioPublicoDto ToPublicoDto(Questionario q) => new(...);
    public static QuestionarioListaDto ToListaDto(Questionario q) => new(...);
    private static PerguntaDto ToPerguntaDto(Pergunta p) => new(...);
}

// QuestionarioService.cs - Focado em lógica de negócio
public class QuestionarioService
{
    public async Task<Result<QuestionarioDto>> CriarQuestionarioAsync(...)
    {
        var questionario = CriarQuestionarioComPerguntas(request, usuarioId);
        await _repository.AdicionarAsync(questionario, ct);
        return Result.Success(QuestionarioMapper.ToDto(questionario)); // ? Usa mapper
    }
}
```

**Benefícios:**
- ? Separação clara de responsabilidades
- ? Mapper reutilizável
- ? Service focado em lógica de negócio
- ? Fácil testar mapeamento isoladamente

---

## ?? Estrutura Final

```
QuestionarioOnline.Application/
??? Services/
    ??? QuestionarioService.cs        ? Lógica de negócio
    ??? QuestionarioMapper.cs         ? Mapeamento (internal static)
    ??? RespostaService.cs
    ??? AuthService.cs
```

---

## ?? Implementação

### QuestionarioMapper.cs (Novo)

```csharp
using QuestionarioOnline.Application.DTOs.Responses;
using QuestionarioOnline.Domain.Entities;

namespace QuestionarioOnline.Application.Services;

internal static class QuestionarioMapper
{
    public static QuestionarioDto ToDto(Questionario q) => new(
        q.Id, 
        q.Titulo, 
        q.Descricao, 
        q.Status.ToString(),
        q.PeriodoColeta.DataInicio, 
        q.PeriodoColeta.DataFim, 
        q.DataCriacao, 
        q.DataEncerramento,
        q.Perguntas.Select(ToPerguntaDto).ToList());

    public static QuestionarioPublicoDto ToPublicoDto(Questionario q) => new(
        q.Id, 
        q.Titulo, 
        q.Descricao,
        q.Perguntas.Select(ToPerguntaDto).ToList());

    public static QuestionarioListaDto ToListaDto(Questionario q) => new(
        q.Id, 
        q.Titulo, 
        q.Status.ToString(),
        q.PeriodoColeta.DataInicio, 
        q.PeriodoColeta.DataFim, 
        q.Perguntas.Count);

    private static PerguntaDto ToPerguntaDto(Pergunta p) => new(
        p.Id, 
        p.Texto, 
        p.Ordem, 
        p.Obrigatoria,
        p.Opcoes.Select(o => new OpcaoDto(o.Id, o.Texto, o.Ordem)).ToList());
}
```

**Design Decisions:**
- ? `internal static` - Usado apenas dentro da Application
- ? Métodos `static` - Sem estado, puro mapeamento
- ? Expression-bodied members - Conciso e legível
- ? Método privado `ToPerguntaDto` - Reutilizado internamente

---

### QuestionarioService.cs (Atualizado)

```csharp
public class QuestionarioService : IQuestionarioService
{
    // Lógica de negócio focada
    public async Task<Result<QuestionarioDto>> CriarQuestionarioAsync(...)
    {
        var questionario = CriarQuestionarioComPerguntas(request, usuarioId);
        await _questionarioRepository.AdicionarAsync(questionario, cancellationToken);
        return Result.Success(QuestionarioMapper.ToDto(questionario)); // ?
    }

    public async Task<QuestionarioPublicoDto?> ObterQuestionarioPublicoAsync(...)
    {
        var questionario = await _questionarioRepository.ObterPorIdComPerguntasAsync(...);
        if (questionario is null) return null;

        try
        {
            questionario.GarantirQuePodeReceberRespostas();
            return QuestionarioMapper.ToPublicoDto(questionario); // ?
        }
        catch (DomainException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<QuestionarioListaDto>> ListarQuestionariosPorUsuarioAsync(...)
    {
        var questionarios = await _questionarioRepository.ObterTodosPorUsuarioAsync(...);
        return questionarios.Select(QuestionarioMapper.ToListaDto); // ?
    }
}
```

---

## ?? Comparação

| Aspecto | Antes | Depois |
|---------|-------|--------|
| **Responsabilidades do Service** | Lógica + Mapeamento | Apenas Lógica ? |
| **Linhas no Service** | 220 linhas | 190 linhas (-15%) |
| **Métodos privados de mapeamento** | 4 no Service | 0 no Service ? |
| **Reutilização de mapeamento** | ? Privado no Service | ? Internal na Application |
| **Testabilidade** | Difícil testar mapeamento | ? Fácil testar isoladamente |

---

## ?? Princípios Aplicados

### ? **Single Responsibility Principle**
- **Service**: Orquestra lógica de negócio
- **Mapper**: Converte Entity ? DTO

### ? **Separation of Concerns**
- Lógica de negócio ? Lógica de mapeamento
- Cada classe tem foco claro

### ? **Open/Closed Principle**
- Fácil adicionar novos mapeamentos sem modificar Service
- Exemplo: `ToResumoDto`, `ToDetalhesDto`, etc.

### ? **Don't Repeat Yourself (DRY)**
- `ToPerguntaDto` reutilizado por todos os mappers
- Evita duplicação de lógica de mapeamento

---

## ?? Testabilidade

### Antes ?
```csharp
// Difícil testar mapeamento isoladamente
// Métodos privados dentro do Service
```

### Depois ?
```csharp
// Fácil testar mapeamento
[Fact]
public void ToDto_DeveMapearcorretamente()
{
    var questionario = QuestionarioFake.Criar();
    
    var dto = QuestionarioMapper.ToDto(questionario);
    
    Assert.Equal(questionario.Id, dto.Id);
    Assert.Equal(questionario.Titulo, dto.Titulo);
    Assert.Equal(questionario.Perguntas.Count, dto.Perguntas.Count);
}
```

---

## ?? Por que NÃO usar AutoMapper?

### Opção 1: AutoMapper
```csharp
// Precisa configurar perfis
CreateMap<Questionario, QuestionarioDto>()
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
    .ForMember(dest => dest.DataInicio, opt => opt.MapFrom(src => src.PeriodoColeta.DataInicio))
    ...
```

**Problemas:**
- ?? Complexidade adicional (perfis, configuração)
- ?? Mapeamentos "mágicos" difíceis de debugar
- ?? Overhead de performance
- ?? Difícil ver o que está sendo mapeado

### Opção 2: Mapper Manual (Nossa escolha ?)
```csharp
public static QuestionarioDto ToDto(Questionario q) => new(
    q.Id, q.Titulo, q.Descricao, q.Status.ToString(), ...);
```

**Benefícios:**
- ? Explícito e fácil de ler
- ? Sem dependências extras
- ? Performance ótima (compile-time)
- ? Type-safe (erros em compile-time)

---

## ? Resultado Final

### Service Focado
```csharp
public class QuestionarioService
{
    // Apenas lógica de negócio
    public async Task<Result<QuestionarioDto>> CriarQuestionarioAsync(...) { }
    public async Task<Result<QuestionarioDto>> EncerrarQuestionarioAsync(...) { }
    public async Task<Result> DeletarQuestionarioAsync(...) { }
    
    // Métodos privados de negócio (não mapeamento)
    private static Questionario CriarQuestionarioComPerguntas(...) { }
    private static bool UsuarioTemPermissao(...) { }
    private async Task<bool> QuestionarioTemRespostas(...) { }
}
```

### Mapper Isolado
```csharp
internal static class QuestionarioMapper
{
    // Apenas mapeamento
    public static QuestionarioDto ToDto(Questionario q) { }
    public static QuestionarioPublicoDto ToPublicoDto(Questionario q) { }
    public static QuestionarioListaDto ToListaDto(Questionario q) { }
}
```

---

## ?? Conclusão

**Separação clara de responsabilidades:**
- ? Service = Lógica de negócio
- ? Mapper = Conversão Entity ? DTO
- ? Código mais limpo e focado
- ? Fácil manter e testar

**Pragmático e simples!** ??
