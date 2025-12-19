# ?? Tratamento de Mensagens do RabbitMQ

## ?? Comportamento do Worker

O Worker processa mensagens do RabbitMQ com as seguintes estratégias:

---

## ? **ACK (Aceitar Mensagem)**

Mensagem é **removida da fila** e **não será reprocessada**:

### Casos:

1. **? Sucesso**
   ```csharp
   // Resposta processada com sucesso
   return; // ACK automático
   ```

2. **? Erro de Negócio** (dados inválidos)
   ```csharp
   if (result.IsFailure)
   {
       _logger.LogError("Erro ao processar resposta: {Error}", result.Error);
       return; // ACK - não adianta reprocessar
   }
   ```
   **Por quê?** Se o questionário não existe ou dados são inválidos, reprocessar 1000x não vai resolver!

3. **? JSON Inválido**
   ```csharp
   catch (JsonException ex)
   {
       _logger.LogError("JSON inválido");
       return; // ACK - JSON sempre será inválido
   }
   ```

---

## ? **NACK (Rejeitar Mensagem)**

Mensagem **volta para a fila** e será **reprocessada**:

### Casos:

1. **?? Erro de Infraestrutura** (temporário)
   ```csharp
   catch (Exception ex)
   {
       _logger.LogError(ex, "Erro inesperado");
       throw; // NACK - reprocessa depois
   }
   ```
   **Exemplos:**
   - Banco de dados offline
   - Timeout de rede
   - Out of memory

   **Por quê?** O problema pode ser temporário. Tentar novamente pode funcionar!

---

## ?? Fluxo de Processamento

```
???????????????????????????????????????????????
? 1. Mensagem chega da fila                  ?
???????????????????????????????????????????????
               ?
???????????????????????????????????????????????
? 2. Deserializar JSON                        ?
?    ? Falha? ? return (ACK)                 ?
???????????????????????????????????????????????
               ?
???????????????????????????????????????????????
? 3. Processar via Service                    ?
?    ? Erro de negócio? ? return (ACK)       ?
?    ??  Erro de infra? ? throw (NACK)        ?
?    ? Sucesso? ? return (ACK)               ?
???????????????????????????????????????????????
```

---

## ?? Estratégia por Tipo de Erro

| Tipo de Erro | Ação | Motivo |
|--------------|------|--------|
| **Sucesso** | ACK | Processado ? |
| **Questionário não existe** | ACK | Reprocessar não vai criar o questionário |
| **Dados inválidos** | ACK | Dados continuarão inválidos |
| **JSON malformado** | ACK | JSON não vai se corrigir sozinho |
| **DB offline** | NACK | Pode voltar online em breve |
| **Timeout** | NACK | Próxima tentativa pode ser mais rápida |
| **Out of Memory** | NACK | Worker pode ter reiniciado com mais memória |

---

## ?? Dead Letter Queue (DLQ)

Mensagens que **falharem muitas vezes** (NACK) vão automaticamente para a **Dead Letter Queue**:

```
respostas-questionario.deadletter
```

### Configuração (RabbitMQ):
- **Max tentativas:** Configurável no RabbitMQ
- **TTL:** Tempo de vida da mensagem
- **DLQ automática:** Criada pelo código

### Ver mensagens na DLQ:
1. Acessar: http://localhost:15672
2. Login: admin/admin123
3. Menu: **Queues** ? **respostas-questionario.deadletter**

---

## ?? Testar Comportamento

### 1. **Enviar mensagem válida** (deve processar e ACK)
```json
{
  "QuestionarioId": "guid-valido",
  "Respostas": [...]
}
```
**Resultado:** ? Processada, removida da fila

### 2. **Enviar JSON inválido** (deve ACK e descartar)
```
{ "invalid json
```
**Resultado:** ? Log de erro, removida da fila (ACK)

### 3. **Enviar questionário inexistente** (deve ACK e descartar)
```json
{
  "QuestionarioId": "00000000-0000-0000-0000-000000000000",
  "Respostas": []
}
```
**Resultado:** ? Log de erro, removida da fila (ACK)

### 4. **Desligar banco de dados** (deve NACK e reprocessar)
```powershell
# Parar SQL Server
net stop MSSQL$LOCALDB
```
**Resultado:** ?? Log de erro, volta para a fila (NACK)

---

## ?? Logs Esperados

### Sucesso:
```
[INFO] Mensagem recebida da fila RabbitMQ
[INFO] Resposta processada com sucesso | RespostaId: abc-123
```

### Erro de Negócio (ACK):
```
[INFO] Mensagem recebida da fila RabbitMQ
[ERROR] Erro ao processar resposta: Questionário não encontrado | QuestionarioId: xyz-789
```

### Erro de Infraestrutura (NACK):
```
[INFO] Mensagem recebida da fila RabbitMQ
[ERROR] Erro inesperado ao processar mensagem
System.Data.SqlClient.SqlException: Timeout expired...
```

---

## ?? Configurações Importantes

### `host.json`:
```json
{
  "extensions": {
    "rabbitMQ": {
      "maxConcurrency": 5,      // Máximo de mensagens processando simultaneamente
      "prefetchCount": 1         // Quantas mensagens buscar por vez
    }
  }
}
```

### `local.settings.json`:
```json
{
  "Values": {
    "RabbitMQConnection": "amqp://admin:admin123@localhost:5672"
  }
}
```

---

## ?? Boas Práticas

? **ACK para erros de negócio** - Não adianta reprocessar dados ruins  
? **NACK para erros temporários** - Pode funcionar na próxima tentativa  
? **Log detalhado** - Facilita debugging  
? **Monitorar DLQ** - Ver mensagens que falharam muito  
? **Idempotência** - Se possível, processar a mesma mensagem 2x não deve causar duplicação  

? **Evitar loops infinitos** - Não fazer NACK para erros permanentes  
? **Evitar throws desnecessários** - Use return para ACK  
