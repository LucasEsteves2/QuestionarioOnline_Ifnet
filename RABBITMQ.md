# RabbitMQ - Guia Rápido

## ?? Início Rápido

### 1. Iniciar RabbitMQ
```powershell
.\start-rabbitmq.ps1
```

### 2. Acessar Management UI
Abra no navegador: http://localhost:15672
- **Usuário:** admin
- **Senha:** admin123

### 3. Parar RabbitMQ
```powershell
.\stop-rabbitmq.ps1
```

## ?? Informações Importantes

### Portas
- **5672** - AMQP (aplicação se conecta aqui)
- **15672** - Management UI (interface web)

### Credenciais Padrão
- **Usuário:** admin
- **Senha:** admin123

### Estrutura Criada Automaticamente
- **Exchange:** `questionario-exchange` (tipo: direct)
- **Dead Letter Exchange:** `questionario-dlx`
- **Queue:** `respostas-questionario`
- **Dead Letter Queue:** `respostas-questionario.deadletter`

## ?? Comandos Úteis

### Ver logs do RabbitMQ
```powershell
docker-compose logs -f rabbitmq
```

### Ver status do container
```powershell
docker ps --filter "name=questionario-rabbitmq"
```

### Entrar no container (shell)
```powershell
docker exec -it questionario-rabbitmq sh
```

### Listar filas (dentro do container)
```sh
rabbitmqctl list_queues
```

### Remover tudo (incluindo dados)
```powershell
docker-compose down -v
```

## ?? Monitoramento no Management UI

### O que você pode ver:
1. **Queues** - Filas e número de mensagens
2. **Exchanges** - Exchanges configurados
3. **Connections** - Conexões ativas
4. **Channels** - Canais abertos
5. **Admin** - Gerenciar usuários e permissões

### Navegação:
- **Overview** - Dashboard geral
- **Connections** - Ver aplicações conectadas
- **Channels** - Ver canais ativos
- **Exchanges** - Ver exchanges e bindings
- **Queues** - Ver filas e mensagens

## ?? Configuração Personalizada

### Alterar credenciais
Edite o arquivo `docker-compose.yml`:
```yaml
environment:
  RABBITMQ_DEFAULT_USER: seu_usuario
  RABBITMQ_DEFAULT_PASS: sua_senha
```

Depois edite `appsettings.json`:
```json
"RabbitMQ": {
  "UserName": "seu_usuario",
  "Password": "sua_senha"
}
```

### Usar em produção
Para produção, altere no `appsettings.Production.json`:
```json
"RabbitMQ": {
  "HostName": "rabbitmq.seudominio.com",
  "Port": "5672",
  "UserName": "usuario_producao",
  "Password": "senha_segura_producao"
}
```

## ?? Troubleshooting

### Porta já em uso
Se a porta 5672 ou 15672 já estiver em uso:
1. Identifique o processo: `netstat -ano | findstr :5672`
2. Mate o processo ou altere a porta no `docker-compose.yml`

### Container não inicia
```powershell
# Ver logs detalhados
docker-compose logs rabbitmq

# Recriar container
docker-compose down
docker-compose up -d --force-recreate
```

### Limpar tudo e recomeçar
```powershell
docker-compose down -v
docker volume prune -f
.\start-rabbitmq.ps1
```

## ?? Recursos Adicionais

- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [RabbitMQ Tutorials](https://www.rabbitmq.com/getstarted.html)
- [Management Plugin Guide](https://www.rabbitmq.com/management.html)

## ? Verificar se está funcionando

### Via PowerShell
```powershell
Invoke-RestMethod -Uri "http://localhost:15672/api/overview" -Method Get -Credential (Get-Credential)
```

### Via curl
```bash
curl -u admin:admin123 http://localhost:15672/api/overview
```

### Na aplicação
Quando você executar a API, verifique os logs:
```
[INF] Conexão RabbitMQ estabelecida com sucesso - Host: localhost:5672, VHost: /
```
