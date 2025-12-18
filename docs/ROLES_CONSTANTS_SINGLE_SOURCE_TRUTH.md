# ? Constantes de Roles: Single Source of Truth

## ?? Problema Identificado

**Usar strings hardcoded para roles viola DRY e cria magic strings!**

### ? **ANTES - Magic Strings**

```csharp
[Authorize(Roles = "Admin")] // ? String hardcoded
[HttpPost]
public async Task<ActionResult> Criar(...) { }

[Authorize(Roles = "Admin")] // ? String hardcoded
[HttpPatch("{id}/status")]
public async Task<ActionResult> Encerrar(...) { }

[Authorize(Roles = "Admin,Analista,Visualizador")] // ? Strings hardcoded
[HttpGet("{id}/resultados")]
public async Task<ActionResult> ObterResultados(...) { }
```

**Problemas:**
- ?? **Magic Strings** - "Admin", "Analista" espalhados pelo código
- ?? **Sem IntelliSense** - Fácil errar ao digitar
- ?? **Manutenção difícil** - Mudar nome de role = buscar em todos os lugares
- ?? **Sem refatoração segura** - Rename não funciona em strings
- ?? **Duplicação** - Enum `UsuarioRole` define roles, mas não é usado

---

## ? **DEPOIS - Constantes Tipadas**

### 1. Classe de Constantes

```csharp
// QuestionarioOnline.Api/Authorization/Roles.cs
using QuestionarioOnline.Domain.Enums;

namespace QuestionarioOnline.Api.Authorization;

public static class Roles
{
    public const string Admin = nameof(UsuarioRole.Admin);
    public const string Analista = nameof(UsuarioRole.Analista);
    public const string Visualizador = nameof(UsuarioRole.Visualizador);
}
```

**Por que `nameof()`?**
- ? **Type-safe** - Erro em compile-time se enum mudar
- ? **Refatoração segura** - Rename funciona
- ? **Single Source of Truth** - Enum define, constante referencia

### 2. Uso nos Controllers

```csharp
using QuestionarioOnline.Api.Authorization;

[Authorize(Roles = Roles.Admin)] // ? Constante tipada
[HttpPost]
public async Task<ActionResult> Criar(...) { }

[Authorize(Roles = Roles.Admin)] // ? Constante tipada
[HttpPatch("{id}/status")]
public async Task<ActionResult> Encerrar(...) { }

[Authorize(Roles = $"{Roles.Admin},{Roles.Analista},{Roles.Visualizador}")] // ? String interpolation com constantes
[HttpGet("{id}/resultados")]
public async Task<ActionResult> ObterResultados(...) { }
```

**Benefícios:**
- ? **IntelliSense** - VS sugere `Roles.Admin`
- ? **Type-safe** - Erro em compile-time
- ? **Refatoração segura** - Rename funciona
- ? **DRY** - Uma única definição
- ? **Single Source of Truth** - Enum é a fonte

---

## ?? Hierarquia de Definição

```
????????????????????????????????????????????????????
? 1. UsuarioRole (Domain/Enums)                    ?
?    - Enum com valores numéricos                  ?
?    - Source of Truth do domínio                  ?
????????????????????????????????????????????????????
                 ?
                 ?
????????????????????????????????????????????????????
? 2. Roles (Api/Authorization)                     ?
?    - Constantes strings usando nameof(Enum)     ?
?    - Ponte entre Domain e ASP.NET Core          ?
????????????????????????????????????????????????????
                 ?
                 ?
????????????????????????????????????????????????????
? 3. Controllers                                   ?
?    - Usa constantes Roles.Admin, etc.           ?
?    - Atributo [Authorize(Roles = ...)]          ?
????????????????????????????????????????????????????
```

---

## ?? Comparação Completa

### Enum do Domain

```csharp
// QuestionarioOnline.Domain/Enums/UsuarioRole.cs
namespace QuestionarioOnline.Domain.Enums;

public enum UsuarioRole
{
    Analista = 1,
    Admin = 2,
    Visualizador = 3
}
```

**Usado em:**
- Entity `Usuario` - `public UsuarioRole Role { get; private set; }`
- JWT Token - Claim "role" com valor da enum

### Constantes da API

```csharp
// QuestionarioOnline.Api/Authorization/Roles.cs
using QuestionarioOnline.Domain.Enums;

namespace QuestionarioOnline.Api.Authorization;

public static class Roles
{
    public const string Admin = nameof(UsuarioRole.Admin);             // "Admin"
    public const string Analista = nameof(UsuarioRole.Analista);       // "Analista"
    public const string Visualizador = nameof(UsuarioRole.Visualizador); // "Visualizador"
}
```

**Usado em:**
- Controllers - Atributo `[Authorize(Roles = Roles.Admin)]`

---

## ?? Por Que NÃO Usar Enum Direto?

### Tentativa 1: Enum no [Authorize] ?

```csharp
[Authorize(Roles = UsuarioRole.Admin)] // ? ERRO: Atributo aceita apenas strings
[HttpPost]
public async Task<ActionResult> Criar(...) { }
```

**Erro de compilação:**
```
CS0029: Cannot implicitly convert type 'UsuarioRole' to 'string'
```

### Tentativa 2: ToString() ?

```csharp
[Authorize(Roles = UsuarioRole.Admin.ToString())] // ? ERRO: ToString() não é constante
[HttpPost]
public async Task<ActionResult> Criar(...) { }
```

**Erro de compilação:**
```
CS0182: An attribute argument must be a constant expression
```

### Solução: nameof() + const ?

```csharp
public const string Admin = nameof(UsuarioRole.Admin); // ? Constante em compile-time

[Authorize(Roles = Roles.Admin)] // ? Funciona perfeitamente
[HttpPost]
public async Task<ActionResult> Criar(...) { }
```

**Por que funciona?**
- ? `nameof()` é avaliado em **compile-time**
- ? Resultado é uma **constante string**
- ? Atributos aceitam constantes

---

## ?? IntelliSense e Refatoração

### IntelliSense

```csharp
[Authorize(Roles = Roles.  // ? VS mostra:
// - Roles.Admin
// - Roles.Analista
// - Roles.Visualizador
```

**Sem constantes:**
```csharp
[Authorize(Roles = "Ad  // ? Nenhuma sugestão, precisa lembrar o nome exato
```

### Refatoração Segura

**Cenário:** Mudar `Admin` para `Administrador` no enum

```csharp
// 1. Renomear enum
public enum UsuarioRole
{
    Analista = 1,
    Administrador = 2, // ? Rename
    Visualizador = 3
}

// 2. Constante atualiza automaticamente
public const string Admin = nameof(UsuarioRole.Administrador); // ? Rename funciona!

// 3. Controllers usam constante (não precisa mudar)
[Authorize(Roles = Roles.Admin)] // ? Continua funcionando
```

**Sem constantes (magic string):**
```csharp
// Precisa buscar e substituir manualmente em TODOS os lugares
[Authorize(Roles = "Admin")] // ? Ainda "Admin", não atualiza automaticamente
```

---

## ?? Uso em Múltiplas Roles

### String Interpolation

```csharp
[Authorize(Roles = $"{Roles.Admin},{Roles.Analista},{Roles.Visualizador}")]
[HttpGet("{id}/resultados")]
public async Task<ActionResult> ObterResultados(...) { }

// Equivalente a:
// [Authorize(Roles = "Admin,Analista,Visualizador")]
```

### Alternativa: String Literal (se preferir)

```csharp
public static class Roles
{
    public const string Admin = nameof(UsuarioRole.Admin);
    public const string Analista = nameof(UsuarioRole.Analista);
    public const string Visualizador = nameof(UsuarioRole.Visualizador);
    
    // Combinações comuns
    public const string TodosAnalistasEVisualizadores = $"{Admin},{Analista},{Visualizador}";
}

// Uso:
[Authorize(Roles = Roles.TodosAnalistasEVisualizadores)]
[HttpGet("{id}/resultados")]
public async Task<ActionResult> ObterResultados(...) { }
```

---

## ?? Onde Usar Constantes vs Enum

| Local | Usa | Motivo |
|-------|-----|--------|
| **Entity Usuario** | `UsuarioRole` (enum) | Domain rico, type-safe |
| **JWT Claim** | `usuario.Role.ToString()` | Serialização para token |
| **Controllers** | `Roles.Admin` (const) | Atributo aceita apenas string |
| **Validação lógica** | `UsuarioRole` (enum) | Type-safe, IntelliSense |
| **Comparação** | `usuario.Role == UsuarioRole.Admin` | Type-safe |

---

## ?? Tabela Resumo

| Abordagem | IntelliSense | Refatoração Segura | Compile-Time Check | DRY |
|-----------|--------------|--------------------|--------------------|-----|
| **Magic Strings** `"Admin"` | ? | ? | ? | ? |
| **Enum Direto** `UsuarioRole.Admin` | ? | ? | ? (não funciona em atributo) | ? |
| **Constantes com nameof()** `Roles.Admin` | ? | ? | ? | ? |

---

## ?? Implementação Final

### Domain (Source of Truth)

```csharp
// QuestionarioOnline.Domain/Enums/UsuarioRole.cs
public enum UsuarioRole
{
    Analista = 1,
    Admin = 2,
    Visualizador = 3
}
```

### API (Constantes)

```csharp
// QuestionarioOnline.Api/Authorization/Roles.cs
using QuestionarioOnline.Domain.Enums;

namespace QuestionarioOnline.Api.Authorization;

public static class Roles
{
    public const string Admin = nameof(UsuarioRole.Admin);
    public const string Analista = nameof(UsuarioRole.Analista);
    public const string Visualizador = nameof(UsuarioRole.Visualizador);
}
```

### Controllers

```csharp
// QuestionarioOnline.Api/Controllers/QuestionarioController.cs
using QuestionarioOnline.Api.Authorization;

[Authorize(Roles = Roles.Admin)]
[HttpPost]
public async Task<ActionResult> Criar(...) { }

[Authorize(Roles = $"{Roles.Admin},{Roles.Analista},{Roles.Visualizador}")]
[HttpGet("{id}/resultados")]
public async Task<ActionResult> ObterResultados(...) { }
```

---

## ? Benefícios Finais

### 1. **Single Source of Truth**
```
UsuarioRole (Enum) ? Roles (Constantes) ? Controllers
```

### 2. **Type Safety**
```csharp
public const string Admin = nameof(UsuarioRole.Admin);
// Se renomear UsuarioRole.Admin, isso quebra em compile-time ?
```

### 3. **IntelliSense**
```csharp
[Authorize(Roles = Roles. // ? Mostra todas as opções
```

### 4. **Refatoração Segura**
```csharp
// Rename UsuarioRole.Admin ? Atualiza automaticamente
```

### 5. **Manutenção Fácil**
```csharp
// Adicionar nova role:
// 1. Adicionar no enum UsuarioRole
// 2. Adicionar constante em Roles
// 3. Usar Roles.NovaRole nos controllers
```

---

## ?? Conclusão

**Constantes de Roles eliminam magic strings e criam Single Source of Truth:**

? **Enum do Domain** - Define roles (source of truth)  
? **Constantes da API** - Ponte type-safe (nameof)  
? **Controllers** - Usam constantes (sem magic strings)  
? **IntelliSense** - Sugestões automáticas  
? **Refatoração segura** - Rename funciona  
? **Compile-time safety** - Erros antes de executar  

**Código agora é type-safe e manutenível!** ??
