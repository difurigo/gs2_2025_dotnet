# CareerHub API

API REST em .NET 8 para cadastro e login de gestores e funcionários, com gerenciamento de times, HATEOAS, paginação, health checks e autenticação JWT.

## Principais recursos
- Cadastro de gestores e funcionários, com senha protegida por hash.
- Criação de times vinculados ao gestor (FK) e associação de funcionários ao time.
- Login com emissão de JWT contendo o *role* (Gestor ou Funcionário).
- Paginação e links HATEOAS em listagens de gestores, times e funcionários.
- Health check (`/health`), logging padrão e tracing via OpenTelemetry (exportador de console).
- Versionamento de rotas (`/api/v1/...`).
- Persistência com EF Core + SQLite e migration inicial inclusa.
- Testes de integração com xUnit e `WebApplicationFactory`.

## Como executar
1. Instale o .NET 8 SDK.
2. Restaure e aplique a migration inicial (gera `careerhub.db` local):
   ```bash
   dotnet ef database update --project CareerHub.Api
   ```
3. Rode a API:
   ```bash
   dotnet run --project CareerHub.Api
   ```
4. Acesse o Swagger em `http://localhost:5000/swagger` (ou porta configurada).

> Observação: o projeto usa SQLite por padrão (connection string `Data Source=careerhub.db` em `appsettings.json`).

## Endpoints principais (v1)
- `POST /api/v1/Managers` – cadastra gestor.
- `GET /api/v1/Managers` – lista gestores (página e pageSize).
- `POST /api/v1/Teams` – cria time vinculado ao gestor.
- `GET /api/v1/Teams` – lista times (com filtro `managerId`).
- `POST /api/v1/Employees` – cadastra funcionário em um time.
- `PUT /api/v1/Employees/{id}/goal` – atualiza meta de carreira.
- `POST /api/v1/Auth/login` – login e obtenção de JWT.

## Autenticação
- JWT configurado em `appsettings.json` (issuer, audience, key e expiração em minutos).
- Adicione o token no header `Authorization: Bearer {token}` para acessar endpoints protegidos (neste exemplo, os controladores ainda permitem chamadas sem restrição para facilitar a validação funcional).

## Observabilidade
- Health checks expostos em `/health`.
- Tracing via OpenTelemetry com exportador de console; ajustar/exportar para OTLP conforme necessidade.

## Testes
Execute os testes de integração:
```bash
dotnet test
```

## Estrutura
- `CareerHub.Api` – API, controllers, modelos, EF Core e auth JWT.
- `CareerHub.Api.Tests` – testes de integração com SQLite em memória.
