# **Avant API – Gestão de Equipes e Planos de Carreira**

API RESTful desenvolvida em .NET 9 para o desafio **GS2**, representando uma solução tecnológica dentro do tema **“O Futuro do Trabalho”**.
A API permite gerenciar **gerentes, funcionários, equipes** e **planos de carreira**, com autenticação JWT, versionamento, paginação e integração com banco Oracle.

---

## **📌 Tecnologias Utilizadas**

* **.NET 9**
* **Entity Framework Core 9**
* **Oracle Database**
* **Swagger / OpenAPI 3**
* **JWT Bearer Authentication**
* **xUnit + WebApplicationFactory (Testes Integrados)**
* **Health Checks**
* **API Versioning (Asp.Versioning)**

---

# **1. Boas Práticas REST (30 pts)**

A API segue padrões REST incluindo:

### ✔ Paginação

Endpoints de listagem retornam dados paginados via query params:

```
?pagina=1&tamanhoPagina=10
```

### ✔ HATEOAS

As respostas paginadas incluem links “self”, “proximo” e “anterior”.

### ✔ Status Codes adequados

* 200 OK
* 201 Created
* 204 No Content
* 400 Bad Request
* 401 Unauthorized
* 403 Forbidden
* 404 Not Found

### ✔ Verbos HTTP implementados

* **GET** (consultas)
* **POST** (cadastro)
* **PUT** (atualizações)
* **DELETE** (remoções)

---

# **2. Monitoramento e Observabilidade (15 pts)**

### ✔ Health Check

Disponível em:

```
/health
```

### ✔ Logging estruturado

Middleware adiciona um **X-Trace-Id** em todas as requisições.

### ✔ Tracing simples

Cada request possui logs com método, path e trace ID.

---

# **3. Versionamento de API (10 pts)**

A API utiliza versionamento via **segmento de URL**, seguindo o padrão:

```
/api/v1/Autenticacao/login
/api/v1/Funcionarios
/api/v1/Equipes
```

Com suporte configurado para futuras versões:

```csharp
options.DefaultApiVersion = new ApiVersion(1, 0);
options.AssumeDefaultVersionWhenUnspecified = true;
options.ReportApiVersions = true;
options.ApiVersionReader = new UrlSegmentApiVersionReader();
```

### ML.NET – Recomendação de Plano de Carreira

A solução utiliza **ML.NET** para demonstrar uma aplicação prática de aprendizado de máquina:

- Classe de serviço: `PlanoCarreiraMlService`
- Biblioteca: `Microsoft.ML` (via NuGet)
- O modelo é treinado em memória com um pequeno conjunto de exemplos que representam perfis distintos de profissionais.
- É utilizado um pipeline de regressão (`Sdca`) para gerar uma pontuação de recomendação de plano de carreira.

Endpoint exposto na **v2** da API:

- **POST** `/api/v2/Funcionarios/recomendacao-plano`
- Request body (exemplo):

  ```json
  {
    "idade": 25,
    "anosExperiencia": 2,
    "cursosConcluidos": 3,
    "nivelAtual": 0,
    "desejaTrabalhoRemoto": 1
  }

---

# **4. Integração e Persistência (30 pts)**

### ✔ Banco de Dados Oracle

Configuração no `appsettings.json` (credenciais omitidas):

```json
"ConnectionStrings": {
  "DefaultConnection": "User Id=USUARIO;Password=SENHA;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=SEU_HOST)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=XE)));"
}
```

### ✔ Entity Framework Core + Migrations

* Migrations criam tabelas automaticamente quando o ambiente **não** é Testing.
* Testes usam **InMemoryDatabase** para não conflitar com Oracle.

---

# **5. Testes automatizados (15 pts)**

Testes integrados implementados com:

* **xUnit**
* **WebApplicationFactory**
* **InMemoryDatabase**
* **HttpClient simulando requisições reais**

Testes cobrem:

✔ Login com credenciais corretas
✔ Login com senha inválida
✔ Registro de gerente
✔ Listagem paginada de funcionários com HATEOAS

Executar testes:

```bash
dotnet test
```

---

# **🚀 Como Rodar o Projeto Localmente**

### 1. Restaurar dependências

```bash
dotnet restore
```

### 2. Rodar a API

```bash
dotnet run --project Avant.Api
```

### 3. Acessar Swagger

```
http://localhost:5008/swagger
```

---

# **🔐 Autenticação**

A API utiliza autenticação via **JWT Bearer**.

Fluxo básico:

1. Registrar gerente → `/api/v1/Autenticacao/registrar-gerente`
2. Fazer login → recebe token JWT
3. Enviar token no header:

```
Authorization: Bearer {token}
```

---

# **📁 Estrutura do Projeto**

```
Avant.Api
 ┣ Controllers
 ┣ Data
 ┣ Dtos
 ┣ Models
 ┣ Services
 ┗ Program.cs
Avant.Api.Tests
 ┣ Integration
 ┗ CustomWebApplicationFactory.cs
```

---

# **🧪 Endpoints principais**

### **Gerentes**

* `POST /api/v1/Autenticacao/registrar-gerente`
* `POST /api/v1/Autenticacao/login`

### **Equipes**

* `POST /api/v1/Equipes`
* `GET /api/v1/Equipes/{id}`

### **Funcionários**

* `POST /api/v1/Funcionarios`
* `GET  /api/v1/Funcionarios`
* `GET  /api/v1/Funcionarios/{id}`
* `PUT  /api/v1/Funcionarios/{id}/plano-carreira`
* `DELETE /api/v1/Funcionarios/{id}`

---

# Testes para os endpoints:

Criar equipe:

```json

{
  "nome": "Equipe 1"
}

```

Criar Funcionário:

```json

{
  "nome": "Roberto Farias",
  "email": "roberto@email.com",
  "senha": "Senha123#",
  "planoCarreira": "TI",
  "equipeId": "<idequipe>"
}

```

---

# **📹 Vídeo de Demonstração**

📌 [https://www.youtube.com/watch?v=YKWjNyH4TTM]

---

# **📎 Links de Entrega**

📁 **Repositório GitHub:**
[https://github.com/difurigo/gs2_2025_dotnet.git]

🔗 **Deploy:**
[https://avantapi-webapp.azurewebsites.net/swagger/index.html]

---
