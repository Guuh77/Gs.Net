# AgroGuard Satellite API

API REST em ASP.NET Core para monitoramento agricola com leituras satelitais, estimativa de risco e alertas preventivos para propriedades rurais.

A proposta une agronegocio, dados espaciais e prevencao de desastres: produtores cadastram fazendas e talhoes, registram ou importam leituras de satelite como NDVI, umidade do solo, temperatura e chuva, e a API gera alertas de seca, incendio, enchente e queda de produtividade.

## Diferencial da solucao

O projeto integra a NASA POWER API para buscar dados agroclimaticos reais por latitude e longitude. A NASA fornece temperatura, precipitacao, umidade relativa, umidade do solo e cobertura de nuvens. Para completar a leitura agricola, o sistema estima o NDVI a partir do perfil da cultura e das condicoes climaticas recentes.

Tambem existe um catalogo de fazendas globais de demonstracao, com 28 areas agricolas em regioes como Brasil, Argentina, Estados Unidos, Canada, Europa, Africa, India, China, Sudeste Asiatico, Australia e Nova Zelandia. O usuario ainda pode cadastrar a propria fazenda e analisar seus talhoes com dados reais da NASA.

## Checklist dos requisitos

| Requisito | Onde esta implementado |
| --- | --- |
| API REST ASP.NET Core | `src/AgroGuard.Api` |
| Clean Architecture | `Domain`, `Application`, `Infrastructure`, `Api`, `Tests` |
| SOLID e DI | `DependencyInjection.cs` nas camadas Application e Infrastructure |
| Tratamento global de excecoes | `src/AgroGuard.Api/Middleware/ExceptionHandlingMiddleware.cs` |
| Banco relacional Oracle com EF Core | `AgroGuardDbContext` + `Oracle.EntityFrameworkCore` |
| Relacionamentos 1:N | usuario-fazendas, fazenda-talhoes, talhao-leituras, talhao-alertas |
| Migrations versionadas | `src/AgroGuard.Infrastructure/Persistence/Migrations` |
| JWT e rotas protegidas | `AuthController`, `JwtTokenService`, `[Authorize]` |
| Autorizacao por papel | criacao de culturas restrita a `Administrator` ou `Analyst` |
| Health checks | `/health` com API e Oracle DB |
| Swagger/OpenAPI | `/swagger` com suporte a Bearer JWT |
| Testes xUnit AAA | `tests/AgroGuard.UnitTests` |
| NASA POWER API | `src/AgroGuard.Infrastructure/Nasa/NasaPowerClient.cs` |

## Arquitetura

```mermaid
flowchart LR
    Client["Cliente / Swagger / Video demo"] --> Api["AgroGuard.Api"]
    Api --> Application["AgroGuard.Application"]
    Application --> Domain["AgroGuard.Domain"]
    Api --> Infrastructure["AgroGuard.Infrastructure"]
    Infrastructure --> Application
    Infrastructure --> Domain
    Infrastructure --> Oracle[("Oracle Database")]

    Domain --> Risk["RiskAssessmentService"]
```

### Responsabilidades

| Camada | Responsabilidade |
| --- | --- |
| Domain | Entidades, enums e regras puras de avaliacao de risco |
| Application | Casos de uso, DTOs, contratos de repositorios e validacoes de fluxo |
| Infrastructure | EF Core Oracle, repositories, JWT, hash de senha e migrations |
| Api | Controllers, Swagger, JWT middleware, health checks e exceptions |
| Tests | Testes unitarios xUnit no padrao Arrange, Act, Assert |

## Modelo de dominio

```mermaid
erDiagram
    USER ||--o{ FARM : owns
    FARM ||--o{ FIELD : contains
    CROP ||--o{ FIELD : defines
    FIELD ||--o{ SATELLITE_READING : receives
    FIELD ||--o{ RISK_ALERT : generates
    SATELLITE_READING ||--o{ RISK_ALERT : triggers

    USER {
        guid Id
        string Name
        string Email
        int Role
    }

    FARM {
        guid Id
        guid OwnerId
        string Name
        string City
        string State
        decimal TotalAreaHectares
    }

    FIELD {
        guid Id
        guid FarmId
        guid CropId
        string Name
        decimal AreaHectares
        string SoilType
    }

    CROP {
        guid Id
        string Name
        decimal IdealNdvi
        decimal WaterDemandIndex
    }

    SATELLITE_READING {
        guid Id
        guid FieldId
        decimal Ndvi
        decimal SoilMoisturePercent
        decimal SurfaceTemperatureCelsius
        decimal RainfallMillimeters
    }

    RISK_ALERT {
        guid Id
        guid FieldId
        int Type
        int Level
        decimal Score
        int Status
    }
```

## Fluxo principal

```mermaid
sequenceDiagram
    participant User as Usuario autenticado
    participant Api as API
    participant App as Application
    participant Domain as Domain
    participant Db as Oracle

    User->>Api: POST /api/fields/{id}/readings
    Api->>App: AddReadingAsync
    App->>Db: Busca talhao e leitura anterior
    App->>Domain: RiskAssessmentService.Assess
    Domain-->>App: Alertas calculados
    App->>Db: Salva leitura e alertas
    Api-->>User: Leitura + alertas gerados
```

## Stack tecnica

- .NET 8 LTS
- ASP.NET Core Web API
- Entity Framework Core 8
- Oracle.EntityFrameworkCore
- JWT Bearer Authentication
- Swagger/OpenAPI
- Health Checks
- xUnit
- Docker Compose para Oracle Free

## Como executar

### 1. Restaurar dependencias

```powershell
dotnet restore AgroGuard.sln
dotnet tool restore
```

### 2. Subir Oracle local

```powershell
docker compose up -d oracle
```

A connection string padrao esta em `src/AgroGuard.Api/appsettings.json`:

```txt
User Id=AGROGUARD;Password=AgroGuard123;Data Source=localhost:1521/FREEPDB1;
```

### 3. Aplicar migrations

```powershell
dotnet tool run dotnet-ef database update --project src/AgroGuard.Infrastructure/AgroGuard.Infrastructure.csproj --startup-project src/AgroGuard.Api/AgroGuard.Api.csproj --context AgroGuardDbContext
```

### 4. Rodar a API

```powershell
dotnet run --project src/AgroGuard.Api/AgroGuard.Api.csproj
```

Depois acesse:

- Interface web: `http://localhost:5218`
- Swagger: `http://localhost:5218/swagger`
- Health check: `http://localhost:5218/health`

## Endpoints principais

| Metodo | Rota | Protecao | Uso |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | Publica | Cria usuario produtor |
| POST | `/api/auth/login` | Publica | Retorna JWT |
| GET | `/api/crops` | JWT | Lista culturas com parametros agronomicos |
| POST | `/api/crops` | JWT + Analyst/Admin | Cria cultura monitorada |
| GET | `/api/farms` | JWT | Lista fazendas do usuario |
| POST | `/api/farms` | JWT | Cadastra fazenda |
| GET | `/api/fields/farm/{farmId}` | JWT | Lista talhoes da fazenda |
| POST | `/api/fields` | JWT | Cadastra talhao |
| POST | `/api/fields/{id}/readings` | JWT | Insere leitura satelital e gera alertas |
| GET | `/api/nasa/global-farms` | JWT | Analisa fazendas globais com dados NASA |
| POST | `/api/nasa/coordinates/analyze` | JWT | Analisa qualquer coordenada selecionada no mapa |
| POST | `/api/nasa/fields/{fieldId}/analyze` | JWT | Gera leitura NASA para um talhao do usuario |
| GET | `/api/alerts` | JWT | Lista alertas |
| GET | `/api/alerts/high-risk` | JWT | Lista alertas altos e criticos |
| PATCH | `/api/alerts/{id}/resolve` | JWT | Resolve alerta |
| GET | `/api/dashboard/summary` | JWT | Retorna resumo operacional |
| GET | `/health` | Publica | Verifica API e Oracle |

## Interface web

A API tambem serve uma interface visual em:

```txt
http://localhost:5218
```

A interface usa os endpoints reais da API:

- login e cadastro com JWT;
- mapa interativo com Leaflet/OpenStreetMap para selecionar fazendas ou coordenadas;
- fazendas globais analisadas com dados reais da NASA;
- analise de qualquer coordenada escolhida pelo usuario;
- exibicao de indicadores agroclimaticos, NDVI estimado e alertas;
- painel de health checks.

Os endpoints de fazendas, talhoes e leituras continuam no backend para cumprir os requisitos tecnicos de relacionamento, persistencia Oracle e arquitetura REST, mas a experiencia principal da entrega foi concentrada no modulo Mundo NASA.

O Swagger continua disponivel em:

```txt
http://localhost:5218/swagger
```

## Exemplo de uso

### Registrar usuario

```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "Produtor Demo",
  "email": "produtor@agroguard.com",
  "password": "AgroGuard123"
}
```

### Criar fazenda

```http
POST /api/farms
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Fazenda Horizonte Verde",
  "city": "Ribeirao Preto",
  "state": "SP",
  "latitude": -21.1775,
  "longitude": -47.8103,
  "totalAreaHectares": 1240.50
}
```

### Enviar leitura satelital

```http
POST /api/fields/{fieldId}/readings
Authorization: Bearer {token}
Content-Type: application/json

{
  "capturedAt": "2026-06-06T12:00:00Z",
  "source": "Sentinel-2 Academic Sample",
  "ndvi": 0.42,
  "soilMoisturePercent": 18,
  "surfaceTemperatureCelsius": 39,
  "rainfallMillimeters": 0,
  "cloudCoveragePercent": 12
}
```

Resposta esperada: a API salva a leitura e retorna alertas como seca, incendio ou queda de produtividade, com score, nivel de risco, descricao e recomendacao.

## Integracao NASA POWER

A integracao esta em:

```txt
src/AgroGuard.Infrastructure/Nasa/NasaPowerClient.cs
```

Parametros consumidos:

| Parametro | Uso no AgroGuard |
| --- | --- |
| `T2M` | Temperatura media |
| `T2M_MAX` | Temperatura maxima para risco de calor/incendio |
| `PRECTOTCORR` | Chuva acumulada para seca ou enchente |
| `RH2M` | Umidade relativa |
| `GWETROOT` | Umidade do solo na zona de raiz |
| `GWETTOP` | Umidade do solo superficial |
| `CLOUD_AMT` | Cobertura de nuvens |

Como a NASA POWER nao entrega NDVI diretamente nesse endpoint, o projeto calcula um NDVI estimado para a demonstracao com base no NDVI ideal da cultura, umidade do solo, calor extremo e chuva recente.

Fontes oficiais usadas:

- NASA POWER Daily API: `https://power.larc.nasa.gov/docs/services/api/temporal/daily/`
- NASA POWER Parameter Dictionary: `https://power.larc.nasa.gov/parameters/`

## Mapa e dados de fazendas

A interface usa Leaflet com OpenStreetMap, porque funciona sem chave paga e sem conta de billing. O Google Maps tambem poderia ser usado, mas a Maps JavaScript API exige API key e billing habilitado.

O projeto usa tres fontes/estrategias:

| Origem | Uso no projeto | Observacao |
| --- | --- | --- |
| Fazendas globais de demonstracao | 28 marcadores pre-carregados no mapa | Criadas para demo academica com coordenadas reais de regioes agricolas |
| Clique livre no mapa | Analise imediata de qualquer latitude/longitude | Usa NASA POWER em tempo real |
| Fazendas do usuario | Cadastro proprio + analise NASA do talhao | Persiste no Oracle |

Nao existe uma API global, gratuita e padronizada que entregue todos os talhoes/fazendas do mundo com limites, dono e cultura. Algumas fontes ajudam parcialmente:

- OpenStreetMap/Overpass pode retornar areas `landuse=farmland` ou `landuse=farmyard`, mas a cobertura varia por regiao e normalmente nao traz cultura ou produtor.
- No Brasil, dados publicos do CAR/SICAR podem trazer poligonos de imoveis rurais, mas nao necessariamente talhoes produtivos ou cultura plantada.
- Nos EUA, dados de Common Land Unit existem, mas nao sao totalmente publicos.

## Regra de risco

A regra principal esta em `RiskAssessmentService` e considera:

- NDVI atual comparado com o NDVI ideal da cultura;
- umidade do solo;
- temperatura de superficie;
- chuva acumulada;
- cobertura de nuvens;
- queda de NDVI em relacao a leitura anterior;
- demanda hidrica da cultura.

Os alertas usam quatro niveis:

- `Low`
- `Moderate`
- `High`
- `Critical`

A API persiste apenas alertas com score relevante, a partir de risco moderado.

## Testes automatizados

Executar:

```powershell
dotnet test AgroGuard.sln
```

Testes implementados:

- gera alerta de seca quando ha estresse hidrico;
- gera alerta de enchente quando ha solo saturado e chuva alta;
- gera alerta de queda produtiva quando o NDVI cai bruscamente;
- nao gera alerta em leitura saudavel;
- resolve alerta aberto corretamente.

Todos seguem o padrao AAA:

```csharp
// Arrange
// Act
// Assert
```

## Migrations

As migrations estao versionadas em:

```txt
src/AgroGuard.Infrastructure/Persistence/Migrations
```

Comando para criar nova migration:

```
powershell
```

cd GS.Net

docker compose up -d oracle

dotnet tool restore

dotnet tool run dotnet-ef database update --project src\AgroGuard.Infrastructure\AgroGuard.Infrastructure.csproj --startup-project src\AgroGuard.Api\AgroGuard.Api.csproj --context AgroGuardDbContext

dotnet run --project src\AgroGuard.Api\AgroGuard.Api.csproj --urls http://localhost:5218
