# NotificacoesService

Microsserviço de notificações do Sistema de Gerenciamento Escolar (SGE). Responsável por processar eventos de outros serviços via RabbitMQ e enviar e-mails para alunos, além de expor uma API interna para consulta de histórico e reenvio manual de notificações.

## Stack

| Tecnologia | Uso |
|---|---|
| .NET 10 | Runtime e framework web |
| ASP.NET Core | API HTTP |
| Entity Framework Core 9 | ORM e migrations |
| PostgreSQL 16 | Banco de dados |
| RabbitMQ 3.13 | Broker de mensagens |
| MailKit 4 | Envio de e-mail via SMTP |
| Razor Templates | Renderização de e-mails HTML |
| Serilog 9 | Logging estruturado JSON |
| Swashbuckle 6 | Documentação OpenAPI/Swagger |
| Docker + Docker Compose | Containerização |

---

## Arquitetura

O serviço segue o padrão **Hexagonal Architecture (Ports & Adapters)**, organizado em 4 projetos:

| Projeto | Responsabilidade |
|---|---|
| `NotificacoesService.API` | Controllers, filtros, middleware, Swagger |
| `NotificacoesService.Application` | Use cases, ports (interfaces), DTOs, Result pattern |
| `NotificacoesService.Domain` | Entidade `Notificacao`, enums, exceções, `INotificacaoRepository` |
| `NotificacoesService.Infrastructure` | Consumers RabbitMQ, repositório EF Core, gateway SMTP, Razor templates |

![Arquitetura Hexagonal](docs/notificacoes-service-architecture-1%20-%20Arquitetura%20Hexagonal.drawio.png)

### Regra de dependência

- **API** usa **Application** via interfaces dos use cases
- **Application** depende apenas de **Domain** — sem nenhuma referência a infraestrutura
- **Infrastructure** implementa as interfaces de **Application** (`IEmailGateway`, `ITemplateRenderer`) e de **Domain** (`INotificacaoRepository`)
- O fluxo de dependência nunca aponta para Infrastructure — **inversão de dependência** em todo o projeto

---

## Endpoints

Todos os endpoints exigem o header `X-Integration` com um cliente autorizado.

**Clientes autorizados:** `matriculas-service` · `notas-service` · `alunos-service` · `admin` · `postman` · `dev`

---

### GET `/api/notificacoes/destinatario/{destinatarioId}`

Retorna o histórico de notificações de um destinatário, ordenado por data de criação decrescente.

| Cenário | HTTP |
|---|---|
| Header ausente ou cliente inválido | `400` |
| `destinatarioId` não é um GUID válido | `400` |
| Destinatário sem notificações | `200 []` |
| Destinatário com notificações | `200 [...]` |

![Fluxo GET Listar Notificações](docs/notificacoes-service-architecture-2%20-%20Fluxo%20GET%20Listar%20Notifica%C3%A7%C3%B5es.drawio.png)

---

### POST `/api/notificacoes/{id}/reenviar`

Reenvia uma notificação com status `Falha` que ainda tenha menos de 3 tentativas.

| Cenário | HTTP |
|---|---|
| Header ausente ou cliente inválido | `400` |
| `{id}` não é um GUID válido | `400` |
| Notificação não encontrada | `404` |
| Notificação já enviada | `409` |
| Limite de 3 tentativas atingido | `409` |
| Falha no envio SMTP | `500` |
| Reenvio bem-sucedido | `204` |

![Fluxo POST Reenviar](docs/notificacoes-service-architecture-3%20-%20Fluxo%20POST%20Reenviar.drawio.png)

---

## Consumo de Mensagens (RabbitMQ)

O serviço consome 3 filas em paralelo. Cada consumer é um `BackgroundService` independente registrado na inicialização da API.

| Fila | Consumer | Use Case | Template |
|---|---|---|---|
| `matricula-realizada` | `MatriculaRealizadaConsumer` | `ProcessarMatriculaRealizadaUseCase` | `MatriculaConfirmada.cshtml` |
| `nota-lancada` | `NotaLancadaConsumer` | `ProcessarNotaLancadaUseCase` | `NotaDisponivel.cshtml` |
| `aluno-atualizado` | `AlunoAtualizadoConsumer` | `ProcessarAlunoAtualizadoUseCase` | `AtualizacaoCadastral.cshtml` |

Ao receber uma mensagem, o fluxo é sempre:

> **Deserializar evento → Criar notificação → Renderizar template → Enviar e-mail → Persistir no banco → BasicAck**

Se o envio falha, a notificação é salva com `status: Falha` e `MotivoFalha` preenchido para reenvio posterior via API.

![Fluxo Consumers RabbitMQ](docs/notificacoes-service-architecture-4%20-%20Fluxo%20Consumers%20RabbitMQ.drawio.png)

---

## Infraestrutura Docker

O ambiente completo é orquestrado via `docker-compose.yml` com 4 serviços na rede `notificacoes-net`.

![Infraestrutura Docker](docs/notificacoes-service-architecture-5%20-%20Infraestrutura%20Docker.drawio.png)

| Container | Imagem | Porta(s) | Descrição |
|---|---|---|---|
| `notificacoes-api` | build local | `8080` | A aplicação |
| `notificacoes-postgres` | `postgres:16-alpine` | `5432` | Banco de dados |
| `notificacoes-rabbitmq` | `rabbitmq:3.13-management-alpine` | `5672` · `15672` | Broker + painel |
| `notificacoes-mailhog` | `mailhog/mailhog:latest` | `1025` · `8025` | SMTP fake + UI |

A API só sobe após postgres e rabbitmq passarem no `healthcheck`. As migrations são aplicadas automaticamente via `MigrateAsync()` no startup. O Dockerfile usa **multi-stage build** e a aplicação roda como usuário não-root (`appuser`).

---

## Como executar

**Pré-requisito:** Docker Desktop instalado e rodando.

```bash
# Subir o ambiente completo
docker compose up --build -d

# Verificar logs da API
docker logs notificacoes-api --tail 30

# Derrubar e limpar volumes
docker compose down -v
```

### URLs de acesso

| Serviço | URL | Credenciais |
|---|---|---|
| API + Swagger | http://localhost:8080/swagger | — |
| RabbitMQ Management | http://localhost:15672 | guest / guest |
| MailHog (e-mails) | http://localhost:8025 | — |

---

## Testes manuais

Consulte o arquivo [`TESTES.md`](TESTES.md) para os 11 casos de uso com comandos `curl` prontos, dados de preparação e respostas esperadas.

### Exemplo rápido (happy path)

```bash
# 1. Inserir notificação com falha
docker exec notificacoes-postgres psql -U postgres -d notificacoes -c "
DELETE FROM notificacoes WHERE id = 'b2c3d4e5-0000-0000-0000-000000000001';
INSERT INTO notificacoes (id, destinatario_id, email, tipo, assunto, corpo, status, tentativas_envio, criada_em, motivo_falha)
VALUES (
  'b2c3d4e5-0000-0000-0000-000000000001',
  '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  'aluno@escola.com.br', 'MatriculaConfirmada',
  'Sua matrícula foi confirmada!', '<p>Parabéns!</p>',
  'Falha', 1, NOW(), 'Timeout de conexão SMTP'
);"

# 2. Consultar (retorna status: Falha)
curl -s -H "X-Integration: dev" \
  http://localhost:8080/api/notificacoes/destinatario/3fa85f64-5717-4562-b3fc-2c963f66afa6

# 3. Reenviar → 204 No Content + e-mail no MailHog
curl -s -w "\nHTTP %{http_code}\n" -X POST -H "X-Integration: dev" \
  http://localhost:8080/api/notificacoes/b2c3d4e5-0000-0000-0000-000000000001/reenviar

# 4. Consultar novamente (retorna status: Enviada)
curl -s -H "X-Integration: dev" \
  http://localhost:8080/api/notificacoes/destinatario/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

---

## Estrutura do projeto

```
notificacoes-service/
├── NotificacoesService.API/
│   ├── Adapters/Input/Http/
│   │   └── NotificacoesController.cs
│   ├── Common/
│   │   └── ApiControllerBase.cs
│   ├── Filters/
│   │   └── XIntegrationHeaderFilter.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs
├── NotificacoesService.Application/
│   ├── Common/           # Result, Error, ErrorType, UseCaseBase
│   ├── DTOs/             # Inputs e Responses
│   ├── Options/          # SmtpOptions, DatabaseOptions, BrokerOptions
│   ├── Ports/
│   │   ├── Input/        # Interfaces dos Use Cases
│   │   └── Output/       # IEmailGateway, ITemplateRenderer
│   └── UseCases/         # 5 use cases
├── NotificacoesService.Domain/
│   ├── Entities/         # Notificacao
│   ├── Enums/            # TipoNotificacao, StatusNotificacao
│   ├── Exceptions/
│   └── Ports/Output/     # INotificacaoRepository
├── NotificacoesService.Infrastructure/
│   ├── Adapters/
│   │   ├── Input/Messaging/   # 3 Consumers + Events
│   │   └── Output/
│   │       ├── Email/         # SmtpEmailGateway
│   │       ├── Persistence/   # DbContext, Repository, Mapper, Configuration
│   │       └── Templates/     # RazorTemplateRenderer + 3 .cshtml
│   └── DependencyInjection.cs
├── docs/                      # Diagramas de arquitetura (PNG + drawio)
├── Dockerfile
├── docker-compose.yml
├── .dockerignore
├── TESTES.md
└── NotificacoesService.slnx
```
