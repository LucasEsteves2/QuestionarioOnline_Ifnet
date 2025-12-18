# ?? Auditoria Completa - Violações de DDD e Clean Architecture

## ? PROBLEMAS ENCONTRADOS

---

## ?? **PROBLEMA 1: BCrypt na Application Layer** (CRÍTICO)

### Localização
`QuestionarioOnline.Application\Services\AuthService.cs`

### Código Problemático
```csharp
public async Task<Result<UsuarioRegistradoDto>> RegistrarAsync(RegistrarUsuarioRequest request)
{
    // ? ERRADO - Biblioteca de infraestrutura na Application
    var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);
    
    var novoUsuario = new Usuario(request.Nome, email, senhaHash);
    // ...
}

public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
{
    // ? ERRADO - Biblioteca de infraestrutura na Application  
    var senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash);
    // ...
}
```

### Por que é errado?

1. **Violação de Clean Architecture**
   - Application não deve conhecer bibliotecas de infraestrutura
   - BCrypt é detalhe de implementação

2. **Violação de Dependency Inversion Principle**
   - Application acoplada a implementação específica
   - Não pode trocar algoritmo de hash sem mudar Application

3. **Dificulta testes**
   - Testes unitários dependem de BCrypt
   - Não pode mockar facilmente

### ? Solução Correta

#### 1. Criar interface na Application

```csharp
// QuestionarioOnline.Application/Interfaces/IPasswordHasher.cs
namespace QuestionarioOnline.Application.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}
```

#### 2. Implementar na Infrastructure

```csharp
// QuestionarioOnline.Infrastructure/Security/BcryptPasswordHasher.cs
using BCrypt.Net;
using QuestionarioOnline.Application.Interfaces;

namespace QuestionarioOnline.Infrastructure.Security;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Verify(password, hashedPassword);
    }
}
```

#### 3. Usar no AuthService

```csharp
// QuestionarioOnline.Application/Services/AuthService.cs
public class AuthService : IAuthService
{
    private readonly IPasswordHasher _passwordHasher; // ? Interface!
    
    public AuthService(
        IUsuarioRepository usuarioRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher) // ? Injetar interface
    {
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UsuarioRegistradoDto>> RegistrarAsync(RegistrarUsuarioRequest request)
    {
        var senhaHash = _passwordHasher.HashPassword(request.Senha); // ? Abstração!
        var novoUsuario = new Usuario(request.Nome, email, senhaHash);
        // ...
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var senhaValida = _passwordHasher.VerifyPassword(request.Senha, usuario.SenhaHash);
        // ...
    }
}
```

#### 4. Registrar no DI

```csharp
// Program.cs ou DependencyInjectionConfig.cs
services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
```

---

## ?? **PROBLEMA 2: AzureQueueStorageAdapter Inexistente** (CRÍTICO)

### Localização
`QuestionarioOnline.CrossCutting\DependencyInjection\DependencyInjectionConfig.cs`

### Código Problemático
```csharp
services.AddSingleton<IMessageQueue>(sp =>
{
    var logger = sp.GetService<ILogger<AzureQueueStorageAdapter>>();
    
    var options = new MessageQueueOptions { /* ... */ };

    // ? ERRO - Classe não existe!
    return new AzureQueueStorageAdapter(storageConnectionString, options, logger);
});
```

### Por que é problema?

- Código não compila em produção
- Referência a classe que não foi criada
- Apenas `InMemoryMessageQueue` existe

### ? Solução

#### Opção 1: Criar o AzureQueueStorageAdapter (Produção)

```csharp
// QuestionarioOnline.Infrastructure/Messaging/AzureQueueStorageAdapter.cs
using Azure.Storage.Queues;
using QuestionarioOnline.Application.Interfaces;
using System.Text.Json;

namespace QuestionarioOnline.Infrastructure.Messaging;

public class AzureQueueStorageAdapter : IMessageQueue
{
    private readonly string _connectionString;
    private readonly MessageQueueOptions _options;
    private readonly ILogger<AzureQueueStorageAdapter> _logger;

    public AzureQueueStorageAdapter(
        string connectionString,
        MessageQueueOptions options,
        ILogger<AzureQueueStorageAdapter> logger)
    {
        _connectionString = connectionString;
        _options = options;
        _logger = logger;
    }

    public async Task SendAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        var queueClient = new QueueClient(_connectionString, queueName);
        await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var messageJson = JsonSerializer.Serialize(message);
        await queueClient.SendMessageAsync(messageJson, cancellationToken: cancellationToken);

        _logger?.LogInformation("Mensagem enviada para fila {QueueName}", queueName);
    }
}
```

#### Opção 2: Usar InMemoryMessageQueue (Desenvolvimento)

```csharp
// DependencyInjectionConfig.cs
services.AddSingleton<IMessageQueue>(sp =>
{
    var logger = sp.GetService<ILogger<InMemoryMessageQueue>>();
    return new InMemoryMessageQueue(); // ? Implementação existente
});
```

---

## ?? **PROBLEMA 3: IQueueHealthMonitor na Infrastructure** (MENOR)

### Localização
`QuestionarioOnline.Infrastructure\Messaging\IQueueHealthMonitor.cs`

### Código Problemático
```csharp
// ?? Interface na Infrastructure
namespace QuestionarioOnline.Infrastructure.Messaging;

public interface IQueueHealthMonitor
{
    Task<QueueMetrics> GetMetricsAsync(string queueName, CancellationToken cancellationToken = default);
    // ...
}
```

### Por que é problema menor?

- Interfaces deveriam estar em Application ou Domain
- Infrastructure não deveria definir contratos
- Mas como é específico de infraestrutura (health check), pode ficar

### ? Solução (Opcional)

Se a interface for usada por Application:

```csharp
// Mover para: QuestionarioOnline.Application/Interfaces/IQueueHealthMonitor.cs
namespace QuestionarioOnline.Application.Interfaces;

public interface IQueueHealthMonitor
{
    Task<QueueMetrics> GetMetricsAsync(string queueName, CancellationToken cancellationToken = default);
    // ...
}
```

Se for apenas para Infrastructure (health endpoints), pode ficar onde está.

---

## ? **O QUE ESTÁ CORRETO**

### 1. Domain Layer ?
```
QuestionarioOnline.Domain/
??? Entities/
?   ??? Usuario.cs          ? Apenas domain logic
?   ??? Questionario.cs     ? Apenas domain logic
?   ??? Resposta.cs         ? Apenas domain logic
??? ValueObjects/
?   ??? Email.cs            ? Validação pura
?   ??? PeriodoColeta.cs    ? Validação pura
??? Enums/
?   ??? UsuarioRole.cs      ? Enum puro
??? Interfaces/
    ??? IUsuarioRepository.cs ? Contrato de persistência
```

**? SEM VIOLAÇÕES!**
- Sem dependências de infraestrutura
- Sem Entity Framework
- Sem BCrypt
- Apenas lógica de negócio pura

### 2. Application Layer ? (Exceto BCrypt)
```
QuestionarioOnline.Application/
??? Services/
?   ??? AuthService.cs      ?? Usa BCrypt diretamente
?   ??? QuestionarioService.cs ? Correto
?   ??? RespostaService.cs     ? Correto
??? Interfaces/
?   ??? IAuthService.cs     ? Contrato correto
?   ??? IJwtTokenService.cs ? Abstração correta
?   ??? IMessageQueue.cs    ? Abstração correta
??? DTOs/                   ? Separados por Request/Response
??? Validators/             ? FluentValidation correto
```

### 3. Infrastructure Layer ?
```
QuestionarioOnline.Infrastructure/
??? Authentication/
?   ??? JwtTokenService.cs  ? Implementação isolada
??? Repositories/           ? Entity Framework isolado
??? Messaging/
?   ??? InMemoryMessageQueue.cs ? Implementação isolada
??? Persistence/
    ??? DbContext.cs        ? EF Core isolado
    ??? Configurations/     ? Fluent API correta
```

### 4. API Layer ?
```
QuestionarioOnline.Api/
??? Controllers/            ? Apenas HTTP
?   ??? AuthController.cs  ? Limpo
?   ??? QuestionarioController.cs ? Limpo
?   ??? RespostaController.cs     ? Limpo
??? Responses/
?   ??? ApiResponse.cs      ? Wrapper HTTP
??? Extensions/
    ??? ClaimsPrincipalExtensions.cs ? Helper correto
```

---

## ?? Resumo da Auditoria

| Categoria | Status | Observação |
|-----------|--------|------------|
| **Domain** | ? EXCELENTE | 100% puro, sem violações |
| **Application** | ?? BOM | BCrypt precisa ser abstraído |
| **Infrastructure** | ?? BOM | Falta criar AzureQueueStorageAdapter |
| **API** | ? EXCELENTE | Limpo, sem lógica de negócio |
| **Clean Architecture** | ?? 90% | 2 problemas críticos |
| **DDD** | ? EXCELENTE | Entidades, VOs, Aggregates corretos |
| **SOLID** | ?? 85% | DIP violado no BCrypt |

---

## ?? Prioridade de Correção

### ?? **CRÍTICO** (Fazer AGORA)
1. ? **Abstrair BCrypt** - Criar IPasswordHasher
2. ? **Criar AzureQueueStorageAdapter** ou usar InMemory

### ?? **OPCIONAL** (Pode fazer depois)
3. ?? Mover `IQueueHealthMonitor` para Application (se for usado lá)

---

## ?? Referências

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [DDD - Eric Evans](https://www.domainlanguage.com/ddd/)
- [Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)

---

## ? Conclusão

**Seu projeto está 90% correto!**

Apenas **2 problemas críticos** encontrados:
1. BCrypt na Application (fácil de corrigir)
2. AzureQueueStorageAdapter faltando (fácil de criar)

**O resto está EXCELENTE:**
- ? Domain puro
- ? Separation of Concerns
- ? Controllers limpos
- ? Result Pattern
- ? Validators na camada correta
- ? JWT Token Service na Infrastructure

**Parabéns pelo projeto bem arquitetado!** ??
