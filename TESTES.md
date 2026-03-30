# Testes manuais — NotificacoesService

Pré-requisito: containers rodando com `docker compose up -d`.

GUIDs fixos usados em todos os exemplos:

| Variável | Valor |
|---|---|
| `destinatarioId` | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |
| `notificacaoId (Falha)` | `b2c3d4e5-0000-0000-0000-000000000001` |
| `notificacaoId (Limite)` | `c3d4e5f6-0000-0000-0000-000000000001` |

> **Importante (Windows):** cole os GUIDs diretamente nos comandos — variáveis bash como `$DEST_ID` não funcionam no PowerShell/CMD.

---

## GET /api/notificacoes/destinatario/{destinatarioId}

### Caso 1 — sem header X-Integration

```bash
curl -s -w "\nHTTP %{http_code}\n" \
  http://localhost:8080/api/notificacoes/destinatario/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

Esperado: `400 Bad Request`

```json
{"error":"Header 'X-Integration' ausente ou cliente não autorizado."}
```

---

### Caso 2 — cliente não autorizado

```bash
curl -s -w "\nHTTP %{http_code}\n" \
  -H "X-Integration: sistema-desconhecido" \
  http://localhost:8080/api/notificacoes/destinatario/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

Esperado: `400 Bad Request`

```json
{"error":"Header 'X-Integration' ausente ou cliente não autorizado."}
```

---

### Caso 3 — GUID inválido na rota

```bash
curl -s -w "\nHTTP %{http_code}\n" \
  -H "X-Integration: dev" \
  http://localhost:8080/api/notificacoes/destinatario/nao-e-um-guid
```

Esperado: `400 Bad Request`

```json
{"errors":{"destinatarioId":["The value 'nao-e-um-guid' is not valid."]}}
```

---

### Caso 4 — destinatário sem notificações (retorna lista vazia)

```bash
curl -s -w "\nHTTP %{http_code}\n" \
  -H "X-Integration: dev" \
  http://localhost:8080/api/notificacoes/destinatario/00000000-0000-0000-0000-000000000099
```

Esperado: `200 OK`

```json
[]
```

---

### Caso 5 — destinatário com notificações (retorna dados reais)

**Passo 1 — preparar os dados:**

```bash
docker exec notificacoes-postgres psql -U postgres -d notificacoes -c "
DELETE FROM notificacoes
  WHERE id IN (
    'b2c3d4e5-0000-0000-0000-000000000001',
    'c3d4e5f6-0000-0000-0000-000000000001'
  );
INSERT INTO notificacoes (id, destinatario_id, email, tipo, assunto, corpo, status, tentativas_envio, criada_em, motivo_falha)
VALUES
  ('b2c3d4e5-0000-0000-0000-000000000001',
   '3fa85f64-5717-4562-b3fc-2c963f66afa6',
   'aluno@escola.com.br', 'MatriculaConfirmada',
   'Sua matrícula foi confirmada!', '<p>Parabéns!</p>',
   'Falha', 1, NOW(), 'Timeout de conexão SMTP'),
  ('c3d4e5f6-0000-0000-0000-000000000001',
   '3fa85f64-5717-4562-b3fc-2c963f66afa6',
   'aluno@escola.com.br', 'NotaDisponivel',
   'Sua nota está disponível', '<p>Acesse o portal.</p>',
   'Falha', 3, NOW(), 'Servidor SMTP indisponível');"
```

**Passo 2 — consultar:**

```bash
curl -s -w "\nHTTP %{http_code}\n" \
  -H "X-Integration: dev" \
  http://localhost:8080/api/notificacoes/destinatario/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

Esperado: `200 OK`

```json
[
  {
    "id": "b2c3d4e5-0000-0000-0000-000000000001",
    "destinatarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "aluno@escola.com.br",
    "tipo": "MatriculaConfirmada",
    "assunto": "Sua matrícula foi confirmada!",
    "status": "Falha",
    "tentativasEnvio": 1,
    "criadaEm": "...",
    "enviadaEm": null,
    "motivoFalha": "Timeout de conexão SMTP"
  },
  {
    "id": "c3d4e5f6-0000-0000-0000-000000000001",
    "destinatarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "aluno@escola.com.br",
    "tipo": "NotaDisponivel",
    "assunto": "Sua nota está disponível",
    "status": "Falha",
    "tentativasEnvio": 3,
    "criadaEm": "...",
    "enviadaEm": null,
    "motivoFalha": "Servidor SMTP indisponível"
  }
]
```

---

## POST /api/notificacoes/{id}/reenviar

### Caso 6 — sem header X-Integration

```bash
curl -s -w "\nHTTP %{http_code}\n" -X POST \
  http://localhost:8080/api/notificacoes/b2c3d4e5-0000-0000-0000-000000000001/reenviar
```

Esperado: `400 Bad Request`

```json
{"error":"Header 'X-Integration' ausente ou cliente não autorizado."}
```

---

### Caso 7 — GUID inválido na rota

```bash
curl -s -w "\nHTTP %{http_code}\n" -X POST \
  -H "X-Integration: dev" \
  http://localhost:8080/api/notificacoes/nao-e-um-guid/reenviar
```

Esperado: `400 Bad Request`

```json
{"errors":{"id":["The value 'nao-e-um-guid' is not valid."]}}
```

---

### Caso 8 — notificação inexistente

```bash
curl -s -w "\nHTTP %{http_code}\n" -X POST \
  -H "X-Integration: dev" \
  http://localhost:8080/api/notificacoes/00000000-0000-0000-0000-000000000000/reenviar
```

Esperado: `404 Not Found`

```json
{"title":"notificacao.nao_encontrada","status":404,"detail":"Notificação não encontrada."}
```

---

### Caso 9 — reenvio bem-sucedido (retorna 204 e envia e-mail)

> Requer o **Caso 5** executado antes (dados no banco com status `Falha`).

```bash
curl -s -w "\nHTTP %{http_code}\n" -X POST \
  -H "X-Integration: dev" \
  http://localhost:8080/api/notificacoes/b2c3d4e5-0000-0000-0000-000000000001/reenviar
```

Esperado: `204 No Content` (corpo vazio)

Verificar o e-mail recebido em: `http://localhost:8025` (MailHog)

Verificar que o status mudou para `Enviada`:

```bash
curl -s -H "X-Integration: dev" \
  http://localhost:8080/api/notificacoes/destinatario/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

Esperado: campo `status` igual a `"Enviada"`, `enviadaEm` preenchido, `motivoFalha` nulo.

---

### Caso 10 — notificação já enviada (requer Caso 9 executado)

```bash
curl -s -w "\nHTTP %{http_code}\n" -X POST \
  -H "X-Integration: dev" \
  http://localhost:8080/api/notificacoes/b2c3d4e5-0000-0000-0000-000000000001/reenviar
```

Esperado: `409 Conflict`

```json
{"title":"notificacao.ja_enviada","status":409,"detail":"A notificação já foi enviada e não pode ser reenviada."}
```

---

### Caso 11 — limite de retentativas atingido

> Requer o **Caso 5** executado antes (notificação com `tentativasEnvio: 3`).

```bash
curl -s -w "\nHTTP %{http_code}\n" -X POST \
  -H "X-Integration: dev" \
  http://localhost:8080/api/notificacoes/c3d4e5f6-0000-0000-0000-000000000001/reenviar
```

Esperado: `409 Conflict`

```json
{"title":"notificacao.limite_retentativas","status":409,"detail":"Limite de retentativas atingido. Não é possível reenviar a notificação."}
```

---

## Resetar dados para repetir os testes

```bash
docker exec notificacoes-postgres psql -U postgres -d notificacoes -c "
DELETE FROM notificacoes
  WHERE id IN (
    'b2c3d4e5-0000-0000-0000-000000000001',
    'c3d4e5f6-0000-0000-0000-000000000001'
  );"
```

Depois execute novamente o **Passo 1 do Caso 5**.

---

## Clientes autorizados (header X-Integration)

| Cliente |
|---|
| `matriculas-service` |
| `notas-service` |
| `alunos-service` |
| `admin` |
| `postman` |
| `dev` |
