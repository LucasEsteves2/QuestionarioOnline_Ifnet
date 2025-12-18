# ?? Guia de Uso - Postman Collection

## ?? Importar Collection

### 1. **Importar Collection**
```
Postman ? Import ? File ? Selecionar "QuestionarioOnline.postman_collection.json"
```

### 2. **Importar Environment**
```
Postman ? Environments ? Import ? Selecionar "QuestionarioOnline.Local.postman_environment.json"
```

### 3. **Selecionar Environment**
```
Postman ? Top-right dropdown ? Selecionar "Questionário Online - Local"
```

---

## ?? Variáveis de Ambiente

| Variável | Valor Padrão | Descrição | Auto-preenchida? |
|----------|--------------|-----------|------------------|
| `baseUrl` | `https://localhost:7001` | URL base da API | ? Manual |
| `authToken` | (vazio) | Token JWT | ? Após Login |
| `questionarioId` | (vazio) | ID do questionário | ? Manual |
| `perguntaId1` | (vazio) | ID da primeira pergunta | ? Manual |
| `opcaoId1` | (vazio) | ID da primeira opção | ? Manual |
| `perguntaId2` | (vazio) | ID da segunda pergunta | ? Manual |
| `opcaoId2` | (vazio) | ID da segunda opção | ? Manual |

---

## ?? Fluxo Completo de Teste

### **Passo 1: Autenticação**

#### 1.1. Registrar Usuário (opcional - se não existir)
```http
POST /api/auth/register
Body:
{
  "nome": "Admin Sistema",
  "email": "admin@questionario.com",
  "senha": "Admin@123"
}
```

**Response Esperado:**
```json
{
  "success": true,
  "data": {
    "id": "guid-do-usuario",
    "nome": "Admin Sistema",
    "email": "admin@questionario.com"
  }
}
```

#### 1.2. Login
```http
POST /api/auth/login
Body:
{
  "email": "admin@questionario.com",
  "senha": "Admin@123"
}
```

**Response Esperado:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "usuarioId": "guid-do-usuario",
    "nome": "Admin Sistema",
    "email": "admin@questionario.com"
  }
}
```

**? Token é AUTOMATICAMENTE salvo na variável `authToken`!**

---

### **Passo 2: Criar Questionário**

```http
POST /api/questionario
Authorization: Bearer {{authToken}}
Body:
{
  "titulo": "Pesquisa de Satisfação 2024",
  "descricao": "Avaliação dos serviços",
  "dataInicio": "2024-01-01T00:00:00Z",
  "dataFim": "2024-12-31T23:59:59Z",
  "perguntas": [
    {
      "texto": "Como você avalia nosso atendimento?",
      "ordem": 1,
      "obrigatoria": true,
      "opcoes": [
        { "texto": "Excelente", "ordem": 1 },
        { "texto": "Bom", "ordem": 2 },
        { "texto": "Regular", "ordem": 3 },
        { "texto": "Ruim", "ordem": 4 }
      ]
    }
  ]
}
```

**Response Esperado:**
```json
{
  "success": true,
  "data": {
    "id": "abc123...",
    "titulo": "Pesquisa de Satisfação 2024",
    "status": "Ativo",
    "perguntas": [
      {
        "id": "pergunta-id-1",
        "texto": "Como você avalia nosso atendimento?",
        "opcoes": [
          { "id": "opcao-id-1", "texto": "Excelente" },
          { "id": "opcao-id-2", "texto": "Bom" }
        ]
      }
    ]
  }
}
```

**?? Copie os IDs:**
1. `questionarioId` = `abc123...`
2. `perguntaId1` = `pergunta-id-1`
3. `opcaoId1` = `opcao-id-1`

**?? Cole nas variáveis do Environment!**

---

### **Passo 3: Listar Questionários**

```http
GET /api/questionario
Authorization: Bearer {{authToken}}
```

**Response Esperado:**
```json
{
  "success": true,
  "data": [
    {
      "id": "abc123...",
      "titulo": "Pesquisa de Satisfação 2024",
      "status": "Ativo",
      "dataInicio": "2024-01-01T00:00:00Z",
      "dataFim": "2024-12-31T23:59:59Z",
      "totalPerguntas": 1
    }
  ]
}
```

---

### **Passo 4: Obter Questionário por ID**

```http
GET /api/questionario/{{questionarioId}}
Authorization: Bearer {{authToken}}
```

**Response:** Questionário completo com todas as perguntas e opções.

---

### **Passo 5: Registrar Resposta**

```http
POST /api/resposta
Authorization: Bearer {{authToken}}
Body:
{
  "questionarioId": "{{questionarioId}}",
  "respostas": [
    {
      "perguntaId": "{{perguntaId1}}",
      "opcaoRespostaId": "{{opcaoId1}}"
    }
  ],
  "estado": "SP",
  "cidade": "São Paulo",
  "regiaoGeografica": "Sudeste"
}
```

**Response Esperado:**
```json
{
  "success": true,
  "data": {
    "id": "resposta-id",
    "questionarioId": "abc123...",
    "dataRegistro": "2024-01-15T10:30:00Z"
  },
  "message": "Resposta recebida e será processada em breve"
}
```

**Status:** `202 Accepted` (processamento assíncrono)

---

### **Passo 6: Obter Resultados**

```http
GET /api/questionario/{{questionarioId}}/resultados
Authorization: Bearer {{authToken}}
```

**Response Esperado:**
```json
{
  "success": true,
  "data": {
    "id": "abc123...",
    "titulo": "Pesquisa de Satisfação 2024",
    "totalRespostas": 1,
    "perguntas": [
      {
        "id": "pergunta-id-1",
        "texto": "Como você avalia nosso atendimento?",
        "opcoes": [
          {
            "id": "opcao-id-1",
            "texto": "Excelente",
            "votos": 1,
            "percentual": 100.0
          },
          {
            "id": "opcao-id-2",
            "texto": "Bom",
            "votos": 0,
            "percentual": 0.0
          }
        ]
      }
    ]
  }
}
```

---

### **Passo 7: Encerrar Questionário**

```http
PATCH /api/questionario/{{questionarioId}}/status
Authorization: Bearer {{authToken}}
```

**Response:** Questionário com status `Encerrado`.

---

### **Passo 8: Deletar Questionário**

```http
DELETE /api/questionario/{{questionarioId}}
Authorization: Bearer {{authToken}}
```

**Response:** `204 No Content`

---

## ?? Autorização por Endpoint

| Endpoint | Método | Autenticação | Roles Permitidas |
|----------|--------|--------------|------------------|
| `/api/auth/register` | POST | ? Não | - |
| `/api/auth/login` | POST | ? Não | - |
| `/api/questionario` | POST | ? Sim | Admin |
| `/api/questionario` | GET | ? Sim | Todos |
| `/api/questionario/{id}` | GET | ? Sim | Todos |
| `/api/questionario/{id}/status` | PATCH | ? Sim | Admin |
| `/api/questionario/{id}` | DELETE | ? Sim | Admin |
| `/api/questionario/{id}/resultados` | GET | ? Sim | Admin, Analista, Visualizador |
| `/api/resposta` | POST | ? Sim | Todos |

---

## ??? Configuração do Token

### Automática (Recomendado)
O token é **automaticamente salvo** após o Login graças ao script de teste:

```javascript
// Script executado após Login (já incluído na collection)
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    if (jsonData.success && jsonData.data && jsonData.data.token) {
        pm.environment.set("authToken", jsonData.data.token);
        console.log("Token salvo:", jsonData.data.token);
    }
}
```

### Manual (se necessário)
1. Faça Login
2. Copie o `token` da response
3. Postman ? Environments ? `authToken` ? Cole o token

---

## ?? Responses Padrão

### ? **Success (200/201/202)**
```json
{
  "success": true,
  "data": { ... },
  "message": "Mensagem opcional"
}
```

### ? **Error (400/401/403/404/500)**
```json
{
  "success": false,
  "error": "Mensagem de erro detalhada"
}
```

### Status Codes
- `200 OK` - Sucesso
- `201 Created` - Recurso criado
- `202 Accepted` - Processamento assíncrono iniciado
- `204 No Content` - Sucesso sem corpo de resposta (DELETE)
- `400 Bad Request` - Erro de validação
- `401 Unauthorized` - Token ausente/inválido
- `403 Forbidden` - Sem permissão (role incorreta)
- `404 Not Found` - Recurso não encontrado
- `500 Internal Server Error` - Erro do servidor

---

## ?? Exemplos de Body

### Criar Questionário Completo
```json
{
  "titulo": "Avaliação de Curso Online",
  "descricao": "Pesquisa de satisfação sobre o curso",
  "dataInicio": "2024-01-01T00:00:00Z",
  "dataFim": "2024-12-31T23:59:59Z",
  "perguntas": [
    {
      "texto": "O conteúdo foi claro?",
      "ordem": 1,
      "obrigatoria": true,
      "opcoes": [
        { "texto": "Sim", "ordem": 1 },
        { "texto": "Não", "ordem": 2 }
      ]
    },
    {
      "texto": "Nota geral do curso",
      "ordem": 2,
      "obrigatoria": true,
      "opcoes": [
        { "texto": "1 estrela", "ordem": 1 },
        { "texto": "2 estrelas", "ordem": 2 },
        { "texto": "3 estrelas", "ordem": 3 },
        { "texto": "4 estrelas", "ordem": 4 },
        { "texto": "5 estrelas", "ordem": 5 }
      ]
    }
  ]
}
```

### Registrar Resposta Completa
```json
{
  "questionarioId": "abc-123-def-456",
  "respostas": [
    {
      "perguntaId": "pergunta-id-1",
      "opcaoRespostaId": "opcao-id-1"
    },
    {
      "perguntaId": "pergunta-id-2",
      "opcaoRespostaId": "opcao-id-5"
    }
  ],
  "estado": "RJ",
  "cidade": "Rio de Janeiro",
  "regiaoGeografica": "Sudeste"
}
```

---

## ?? Troubleshooting

### **Erro: 401 Unauthorized**
- **Causa:** Token ausente ou expirado
- **Solução:** Faça login novamente

### **Erro: 403 Forbidden**
- **Causa:** Usuário sem permissão (role incorreta)
- **Solução:** Certifique-se de estar usando um usuário Admin para criar/encerrar/deletar

### **Erro: 400 Bad Request - "Questionário não está ativo"**
- **Causa:** Tentando responder questionário encerrado
- **Solução:** Use um questionário com status Ativo

### **Erro: 404 Not Found**
- **Causa:** ID de questionário/pergunta/opção incorreto
- **Solução:** Verifique os IDs nas variáveis do Environment

### **Token não salva automaticamente**
- **Causa:** Script de teste não executou
- **Solução:** Postman ? Settings ? General ? Habilitar "Automatically follow redirects"

---

## ? Checklist de Teste Completo

- [ ] 1. Importar Collection
- [ ] 2. Importar Environment
- [ ] 3. Selecionar Environment "Local"
- [ ] 4. Register (criar usuário Admin)
- [ ] 5. Login (token salvo automaticamente)
- [ ] 6. Criar Questionário (copiar IDs para Environment)
- [ ] 7. Listar Questionários
- [ ] 8. Obter Questionário por ID
- [ ] 9. Registrar Resposta (202 Accepted)
- [ ] 10. Obter Resultados (verificar votos e percentuais)
- [ ] 11. Encerrar Questionário
- [ ] 12. Deletar Questionário

---

## ?? Documentação Adicional

- **Swagger UI:** `https://localhost:7001/swagger`
- **Architecture Docs:** Veja pasta `docs/` no repositório
- **Repository:** https://github.com/LucasEsteves2/QuestionarioOnline_Ifnet

---

## ?? Collection Pronta!

**Tudo configurado para testar a API completa!** ??
