# ?? Como Iniciar a Aplicação

Este guia mostra como iniciar a aplicação QuestionarioOnline com RabbitMQ.

## ?? Pré-requisitos

- ? .NET 8 SDK
- ? Docker Desktop  
- ? Azure Functions Core Tools (para Workers)
- ? PowerShell ou Terminal

---

## ?? Iniciar a Aplicação Completa (API + Workers)

### **Opção 1: Visual Studio (RECOMENDADO)**

1. Abra a solution `QuestionarioOnline.sln`
2. Clique com botão direito na Solution
3. Selecione **"Set Startup Projects..."**
4. Escolha **"Multiple startup projects"**
5. Configure:
   - ? `QuestionarioOnline.Api` ? **Start**
   - ? `QuestionarioOnline.Workers.Function` ? **Start**
6. Clique **OK**
7. Inicie RabbitMQ: `docker-compose up -d`
8. Pressione **F5**

### **Opção 2: Script PowerShell (Terminais Separados)**

```powershell
.\start-all.ps1
```

**O que acontece:**
- ? Verifica Docker
- ? Inicia RabbitMQ
- ? Abre terminal para a API
- ? Abre terminal para os Workers (Azure Functions)
- ? Cada um roda independentemente

### **Opção 3: Manual (Passo a Passo)**

```powershell
# Terminal 1: RabbitMQ
docker-compose up -d

# Terminal 2: API
dotnet run --project QuestionarioOnline\QuestionarioOnline.Api.csproj

# Terminal 3: Workers (Azure Functions)
cd QuestionarioOnline.Workers.Function
func start
# OU
dotnet run --project QuestionarioOnline.Workers.Function\QuestionarioOnline.Workers.Function.csproj
```

---

## ?? Iniciar Apenas API (Sem Workers)

### 1?? Iniciar RabbitMQ

Na pasta raiz do projeto, execute:

```powershell
docker-compose up -d
```

**O que acontece:**
- ? Baixa a imagem RabbitMQ (primeira vez - ~2-5 minutos)
- ? Cria e inicia o container
- ? RabbitMQ fica pronto em ~15 segundos

### 2?? Iniciar a API

```powershell
dotnet run --project QuestionarioOnline\QuestionarioOnline.Api.csproj
```

Ou no Visual Studio: Pressione `F5`

---

## ?? URLs Disponíveis

Quando tudo estiver rodando:

| Serviço | URL | Observações |
|---------|-----|-------------|
| **API** | https://localhost:7001 | REST API |
| **Swagger** | https://localhost:7001/swagger | Documentação interativa |
| **RabbitMQ UI** | http://localhost:15672 | Login: admin/admin123 |
| **Workers** | (Background) | Azure Functions processando filas |

---

## ?? Fluxo Completo

```
1. Usuário envia resposta via API
   ?
2. API valida e envia para fila RabbitMQ (respostas-questionario)
   ?
3. Azure Functions (Workers) escutam a fila via RabbitMQ Trigger
   ?
4. Workers processam automaticamente
   ?
5. Workers salvam no banco de dados
```

---

## ?? Arquitetura do Worker

O projeto `QuestionarioOnline.Workers.Function` usa **Azure Functions v4** com **RabbitMQ Trigger**:

```csharp
[Function(nameof(ProcessarRespostaFunction))]
public async Task Run(
    [RabbitMQTrigger("respostas-questionario", ConnectionStringSetting = "RabbitMQConnection")] 
    string message)
{
    // Processa mensagem automaticamente
}
```

### Configuração (`local.settings.json`):
```json
{
  "Values": {
    "RabbitMQConnection": "amqp://admin:admin123@localhost:5672"
  }
}
```

---

## ?? Parar a Aplicação

### Parar API
Pressione `Ctrl+C` no terminal da API

### Parar Workers
Pressione `Ctrl+C` no terminal dos Workers

### Parar RabbitMQ

```powershell
docker-compose down
```

Para remover volumes também (limpar dados):
```powershell
docker-compose down -v
```

---

## ?? Comandos Docker Úteis

```powershell
# Ver status
docker ps

# Ver logs do RabbitMQ
docker logs -f questionario-rabbitmq

# Reiniciar RabbitMQ
docker restart questionario-rabbitmq

# Parar RabbitMQ
docker stop questionario-rabbitmq

# Iniciar RabbitMQ novamente
docker start questionario-rabbitmq
```

---

## ?? Desenvolvimento com Hot Reload

```powershell
# Terminal 1: RabbitMQ
docker-compose up -d

# Terminal 2: API com hot reload
dotnet watch run --project QuestionarioOnline\QuestionarioOnline.Api.csproj

# Terminal 3: Workers com hot reload
cd QuestionarioOnline.Workers.Function
func start --csharp
```

---

## ?? Troubleshooting

### Problema: "Cannot connect to Docker daemon"
**Solução:** Inicie o Docker Desktop e aguarde alguns segundos

### Problema: "Porta 5672 já está em uso"
**Solução:**
```powershell
# Ver o que está usando a porta
netstat -ano | findstr :5672

# Parar container
docker stop questionario-rabbitmq
```

### Problema: "RabbitMQConnection not found"
**Solução:**
```powershell
# Verifique se o arquivo local.settings.json existe
# E se contém a chave RabbitMQConnection
```

### Problema: Workers não processam mensagens
**Solução:** 
```powershell
# 1. Verificar se Workers estão rodando
# 2. Verificar logs dos Workers no terminal
# 3. Verificar RabbitMQConnection no local.settings.json
# 4. Acessar RabbitMQ UI e verificar se há mensagens na fila
#    http://localhost:15672 ? Queues ? respostas-questionario
```

### Problema: "Azure Functions Core Tools not found"
**Solução:**
```powershell
# Instalar via npm
npm install -g azure-functions-core-tools@4

# OU instalar via chocolatey
choco install azure-functions-core-tools-4

# OU instalar via MSI
# Download: https://docs.microsoft.com/azure/azure-functions/functions-run-local
```

---

## ?? Testes

```powershell
# Rodar todos os testes
dotnet test

# Build
dotnet build
```

---

## ?? Configuração

### Workers (`local.settings.json`)
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "RabbitMQConnection": "amqp://admin:admin123@localhost:5672"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QuestionarioOnlineDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### API (`appsettings.json`)
```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": "5672",
    "UserName": "admin",
    "Password": "admin123"
  }
}
```

---

## ? Checklist Rápido (API + Workers)

1. [ ] Docker Desktop instalado e rodando
2. [ ] Azure Functions Core Tools instalado
3. [ ] Executar `docker-compose up -d`
4. [ ] Aguardar 15 segundos
5. [ ] Iniciar API: `dotnet run --project QuestionarioOnline\QuestionarioOnline.Api.csproj`
6. [ ] Iniciar Workers: `func start` (dentro da pasta Workers)
7. [ ] Acessar https://localhost:7001/swagger
8. [ ] Testar endpoint de registro de resposta
9. [ ] Verificar fila no RabbitMQ: http://localhost:15672
10. [ ] Verificar logs dos Workers
11. [ ] Confirmar que mensagem foi processada (fila vazia)

---

## ?? Documentação Adicional

- [Documentação Completa RabbitMQ](RABBITMQ.md)
- [Configurar Visual Studio](CONFIGURAR-VISUAL-STUDIO.md)
- [Swagger UI](https://localhost:7001/swagger)
- [RabbitMQ Management](http://localhost:15672)
- [Azure Functions + RabbitMQ](https://learn.microsoft.com/azure/azure-functions/functions-bindings-rabbitmq)

---

## ?? Tecnologias Utilizadas

- **API**: ASP.NET Core 8 REST API
- **Workers**: Azure Functions v4 com RabbitMQ Trigger
- **Message Broker**: RabbitMQ 3.13
- **Database**: SQL Server (LocalDB)
- **Container**: Docker + Docker Compose

---

## ?? Scripts Disponíveis

| Script | Descrição |
|--------|-----------|
| `start-all.ps1` | Inicia RabbitMQ + API + Workers (terminais separados) |
| `docker-compose up -d` | Apenas RabbitMQ |
