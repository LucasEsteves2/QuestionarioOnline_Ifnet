# ?? Postman Collection - Questionário Online API

## ?? Quick Start

### 1. **Importar no Postman**
```
?? postman/
  ??? QuestionarioOnline.postman_collection.json    ? Importar Collection
  ??? QuestionarioOnline.Local.postman_environment.json    ? Importar Environment
  ??? README.md    ? Guia completo
```

### 2. **Selecionar Environment**
```
Postman ? Top-right ? "Questionário Online - Local"
```

### 3. **Fazer Login**
```
POST /api/auth/login
Body:
{
  "email": "admin@questionario.com",
  "senha": "Admin@123"
}
```

**? Token é AUTOMATICAMENTE salvo!**

---

## ?? Endpoints Disponíveis

### **Auth (2)**
- `POST /api/auth/register` - Criar usuário
- `POST /api/auth/login` - Login (token auto-save)

### **Questionário (6)**
- `POST /api/questionario` - Criar (Admin)
- `GET /api/questionario` - Listar todos
- `GET /api/questionario/{id}` - Obter por ID
- `PATCH /api/questionario/{id}/status` - Encerrar (Admin)
- `DELETE /api/questionario/{id}` - Deletar (Admin)
- `GET /api/questionario/{id}/resultados` - Resultados (Admin/Analista/Visualizador)

### **Resposta (1)**
- `POST /api/resposta` - Registrar resposta (202 Accepted - assíncrono)

---

## ?? Variáveis de Ambiente

| Variável | Valor Padrão | Auto-preenchida? |
|----------|--------------|------------------|
| `baseUrl` | `https://localhost:7001` | ? Manual |
| `authToken` | (vazio) | ? Após Login |
| `questionarioId` | (vazio) | ? Manual |
| `perguntaId1` | (vazio) | ? Manual |
| `opcaoId1` | (vazio) | ? Manual |

---

## ?? Fluxo de Teste Rápido

1. **Login** ? Token salvo automaticamente
2. **Criar Questionário** ? Copiar IDs (questionarioId, perguntaId, opcaoId)
3. **Colar IDs** nas variáveis do Environment
4. **Registrar Resposta** ? 202 Accepted
5. **Obter Resultados** ? Verificar votos e percentuais

---

## ?? Documentação Completa

?? **Leia:** [`postman/README.md`](postman/README.md)

**Inclui:**
- ? Guia passo a passo completo
- ? Exemplos de body para todos os endpoints
- ? Troubleshooting
- ? Autorização por endpoint
- ? Checklist de teste

---

## ?? Collection Completa!

**Todos os endpoints configurados e prontos para testar!** ??
