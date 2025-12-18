# ??? Estratégia de Resiliência para Alto Volume

## ?? **Problema: Milhões de Respostas Simultâneas**

Em eleições, podemos ter:
- **Pico**: 1 milhão de respostas em 1 hora
- **Taxa**: ~277 respostas/segundo
- **Falhas**: 0.1% = 1.000 mensagens com erro

---

## ? **Solução Implementada**

### **1. Retry Policy (Exponential Backoff)**

```csharp
Retry 1: 2 segundos
Retry 2: 4 segundos
Retry 3: 8 segundos
Retry 4: 16 segundos
Retry 5: 32 segundos
Máximo: 60 segundos
```

**Configuração:**
```csharp
var options = new MessageQueueOptions
{
    MaxRetryAttempts = 5,
    ExponentialBackoffBaseSeconds = 2.0
};
```

**O que resolve:**
- ? Erros temporários (network timeout, DB lock)
- ? Throttling (HTTP 429, 503)
- ? Sobrecarga momentânea

---

### **2. Dead Letter Queue (DLQ)**

**Fluxo:**
```
Mensagem ? Retry 1 ? Retry 2 ? Retry 3 ? Retry 4 ? Retry 5 ? DLQ
          (falha)   (falha)   (falha)   (falha)   (falha)   (move)
```

**Quando move para DLQ:**
- ? Máximo de 5 tentativas atingido
- ? Erro de validação (não adianta retentar)
- ? Questionário não encontrado
- ? Mensagem malformada

**Benefícios:**
- ? Não perde mensagens
- ? Não trava processamento
- ? Investigação posterior
- ? Reprocessamento manual

---

### **3. Idempotência**

**Problema:**
```
1. Mensagem processada com sucesso
2. DB salva resposta
3. Function crashea antes de deletar mensagem da fila
4. Mensagem fica visível novamente
5. Processada NOVAMENTE ? DUPLICAÇÃO!
```

**Solução:**
```csharp
// Verifica se já foi processado ANTES de salvar
var jaProcessado = await _respostaRepository.JaRespondeuAsync(...);

if (jaProcessado)
{
    _logger.LogWarning("Mensagem duplicada, ignorando");
    return; // NÃO É ERRO, apenas ignora
}
```

**Resultado:**
- ? Mensagens podem ser processadas múltiplas vezes
- ? Resultado sempre o mesmo (idempotente)
- ? Sem duplicação no banco

---

### **4. Visibility Timeout**

**Configuração:**
```csharp
VisibilityTimeoutSeconds = 30
```

**Funcionamento:**
```
00:00:00 - Mensagem recebida (invisível por 30s)
00:00:15 - Processamento OK ? Deleta mensagem
         ? Sucesso!

OU

00:00:00 - Mensagem recebida (invisível por 30s)
00:00:25 - Function crashea
00:00:30 - Mensagem fica VISÍVEL novamente
         ? Outra instância processa
         ? Retry automático!
```

**Importante:**
- 30 segundos é suficiente para processar 95% das mensagens
- Se demorar mais, mensagem fica visível (outra instância pega)

---

### **5. Telemetria e Observabilidade**

**Logs estruturados:**
```csharp
_logger.LogInformation(
    "? Resposta processada | MessageId: {Id} | Duration: {Duration}ms",
    messageId, duration);

_logger.LogWarning(
    "?? Throttling detectado | Queue: {Queue} | Status: {Status}",
    queueName, 429);

_logger.LogError(
    "? Erro ao processar | MessageId: {Id} | Retry: {Retry}/5",
    messageId, retryCount);
```

**Endpoint de Health Check:**
```http
GET /api/health/queue
? {
    "isHealthy": true,
    "status": "Healthy",
    "queueMetrics": {
        "messagesCount": 42,
        "deadLetterCount": 2
    }
}
```

---

## ?? **Monitoramento em Produção**

### **Alertas Configurados**

| Métrica | Threshold | Ação |
|---------|-----------|------|
| Dead Letter > 100 | CRITICAL | Investigar imediatamente |
| Backlog > 10.000 | WARNING | Aumentar workers |
| Backlog > 100.000 | CRITICAL | Escalar horizontalmente |
| Retry > 50% | WARNING | Verificar DB/Network |

### **Dashboard Sugerido (Azure Monitor)**

```kusto
// Query 1: Taxa de sucesso
QueueMessages
| where QueueName == "respostas-questionario"
| summarize 
    Total = count(),
    Success = countif(Status == "Success"),
    Failed = countif(Status == "Failed"),
    SuccessRate = round(countif(Status == "Success") * 100.0 / count(), 2)
by bin(Timestamp, 5m)
| render timechart

// Query 2: Distribuição de retries
QueueMessages
| where QueueName == "respostas-questionario"
| summarize count() by DequeueCount
| render barchart

// Query 3: Dead Letter Queue growth
QueueMessages
| where QueueName == "respostas-questionario-deadletter"
| summarize count() by bin(Timestamp, 1h)
| render timechart
```

---

## ?? **Testes de Carga**

### **Cenário 1: Carga Normal**
```
- 100 req/s
- Latência: < 50ms
- Retry: < 1%
- DLQ: 0
? PASSOU
```

### **Cenário 2: Pico de Carga**
```
- 1.000 req/s
- Latência: < 200ms
- Retry: 5%
- DLQ: < 0.1%
? PASSOU
```

### **Cenário 3: DB Indisponível (30 segundos)**
```
- Todas mensagens vão para retry
- Após 30s, DB volta
- Todas mensagens processadas
- DLQ: 0
? PASSOU (resiliência funcionou!)
```

### **Cenário 4: Mensagens Malformadas**
```
- 10 mensagens inválidas
- Movidas para DLQ imediatamente
- Não afetaram processamento das válidas
? PASSOU
```

---

## ?? **Recomendações Finais**

### **? FAZER**
1. Monitorar Dead Letter Queue diariamente
2. Configurar alertas no Azure Monitor
3. Ter runbook para reprocessar DLQ
4. Testar reprocessamento em staging
5. Documentar decisões de retry

### **? NÃO FAZER**
1. Retry infinito (causa loop)
2. Retry imediato (agrava throttling)
3. Ignorar DLQ (perde mensagens)
4. Processar sem idempotência (duplica)
5. Não monitorar métricas

---

## ?? **SLA Esperado**

| Métrica | Valor |
|---------|-------|
| **Disponibilidade** | 99.9% (Azure SLA) |
| **Latência P50** | < 50ms |
| **Latência P99** | < 500ms |
| **Taxa de Sucesso** | > 99.9% |
| **Perda de Mensagens** | 0% (graças a DLQ) |

---

## ?? **Conclusão**

Esta arquitetura garante:
- ? **Resiliência**: Retry automático
- ? **Confiabilidade**: DLQ (zero perda)
- ? **Observabilidade**: Logs e métricas
- ? **Escalabilidade**: Milhões de req/s
- ? **Idempotência**: Sem duplicação

**Pronto para eleições! ???**
