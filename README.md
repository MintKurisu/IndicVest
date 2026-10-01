# IndicVest

![CI](https://github.com/MintKurisu/IndicVest/actions/workflows/ci.yml/badge.svg)

IndicVest is a full-stack country investment ranking system built with an ASP.NET Core Web API (.NET 9), Onion Architecture and a React frontend. Analysts register macroeconomic indicators per country and year, and the system normalizes them with Min-Max scaling, applies configurable weights, and produces a ranked list of countries together with an estimated return rate for each one. A simulation mode lets analysts test alternative weights without touching the stored configuration.

The project is a complete rewrite of an earlier ASP.NET Core MVC application, redesigned as a REST API with a dedicated frontend, generic repositories and services, centralized error handling, and unit and integration test suites.

## Features

### Data Management

- Country management (name and ISO code)
- Macro indicator management (name, weight, and whether a higher value is better)
- Indicator values registered per country, macro indicator and year
- Only one value per macro indicator, country and year is allowed
- Indicator listing with filters by country and year
- Configurable minimum and maximum estimated return rates

### Ranking

- Ranking generated for a selected year, defaulting to the most recent year with data
- Eligibility validation per country before ranking
- Min-Max normalization that respects the direction of each indicator
- Weighted scoring and estimated return rate per country
- Results sorted by score, from highest to lowest

### Simulation

- Choose any subset of the existing macro indicators
- Assign simulation-specific weights, independent from the stored ones
- Computed on demand, nothing is persisted and stored data is never modified

### Data Integrity

- The total weight of macro indicators can never exceed 1
- A ranking is only generated when the weights add up to exactly 1
- A ranking requires at least two eligible countries
- Cascade deletion with a dependency preview and explicit confirmation (see [Design Decisions](#design-decisions))

### API and Quality

- Global exception handling via ProblemDetails (RFC 7807)
- Request validation with FluentValidation
- Swagger documentation with SwaggerOperation annotations
- Unit tests for services and integration tests for repositories
- Continuous integration with GitHub Actions

### Frontend

- Dark, terminal-style financial dashboard with monospace metrics
- One page per entity with drawer forms for creating and editing
- Confirmation modal showing dependent data before a cascade delete
- Server state handled with TanStack React Query

## Tech Stack

| Layer | Technology |
|---|---|
| Backend framework | ASP.NET Core Web API (.NET 9) |
| ORM | Entity Framework Core (Code First) |
| Database | PostgreSQL on Supabase |
| Validation | FluentValidation |
| Object mapping | AutoMapper |
| Error handling | IExceptionHandler with ProblemDetails |
| API documentation | Swagger / OpenAPI |
| Testing | xUnit, Moq, FluentAssertions, SQLite in-memory |
| CI | GitHub Actions |
| Frontend | React with Vite |
| Data fetching | TanStack React Query and Axios |

## Architecture

IndicVest follows Onion Architecture. Dependencies point inward: outer layers depend on inner layers, never the opposite.

```
IndicVest/
├── backend/
│   ├── IndicVest.Core.Domain                  → Entities, repository interfaces, domain exceptions
│   ├── IndicVest.Core.Application             → Services, DTOs, validators, AutoMapper profiles
│   ├── IndicVest.Infrastructure.Persistence   → DbContext, EF Core configurations, repositories, migrations
│   ├── IndicVestWebApi                        → Controllers, exception handler, DI composition root
│   └── (test projects)                        → Unit and integration tests
└── frontend/                                  → React + Vite application
```

Key patterns:

- **Generic repository and generic service.** Base CRUD logic is written once. Specific repositories and services extend the generic ones and only add or override what is particular to their entity.
- **DTOs everywhere.** Entities are never exposed through the API. AutoMapper profiles are separated per entity.
- **Centralized error handling.** A `GlobalExceptionHandler` converts domain exceptions such as `NotFoundException` and `ValidationException` into ProblemDetails responses, so controllers contain no try/catch blocks.
- **Validation at the boundary.** FluentValidation validators are registered by assembly scanning and run before business logic.
- **Code First.** The database schema is defined by the domain entities and evolved through EF Core migrations.

### Domain Model

- **Country**: a country that can be ranked.
- **MacroIndicator**: the definition of an economic indicator, with its weight and direction (higher is better or lower is better).
- **Indicator**: the value of a macro indicator for a country in a given year.
- **ReturnRate**: the minimum and maximum estimated return used to convert a score into a return rate. Seeded with defaults of 0.02 and 0.15 (2% and 15%).

## How the Ranking Works

### Eligibility

For the selected year, a country is eligible only if it has a registered indicator for every macro indicator that has a weight greater than 0. Macro indicators with weight 0 are ignored. The ranking is only produced when the macro indicator weights sum to 1 and at least two countries are eligible.

### Scoring Algorithm

1. **Normalization** (Min-Max scaling across the eligible countries)
   - Higher is better: `(value - min) / (max - min)`
   - Lower is better: `(max - value) / (max - min)`
   - If `min == max`: `0.5`
2. **Weighted score**: `sub_score = normalized_value x weight`
3. **Final score**: `score = sum(sub_scores)`, always between 0 and 1
4. **Return rate**: `r = r_min + (r_max - r_min) x score`

### Worked Example

Three countries, two indicators for the same year: GDP per capita (weight 0.6, higher is better) and inflation (weight 0.4, lower is better). Return rate range of 2% to 15%.

| Country | GDP per capita | Inflation | Normalized GDP | Normalized inflation | Score | Estimated return |
|---|---|---|---|---|---|---|
| A | 40000 | 5.0 | 0.5 | 1.0 | 0.70 | 11.10% |
| C | 50000 | 10.0 | 1.0 | 0.0 | 0.60 | 9.80% |
| B | 30000 | 8.0 | 0.0 | 0.4 | 0.16 | 4.08% |

The table is already sorted the way the ranking is returned: by score, from highest to lowest.

## Design Decisions

### Cascade deletion with explicit confirmation

Deleting a country or a macro indicator that already has indicator values attached is a destructive operation. Instead of silently deleting related data or always blocking the request, the API supports both behaviors and lets the client choose:

- `DELETE /Country/{id}` and `DELETE /MacroIndicator/{id}` accept a `cascade` query parameter that defaults to `false`.
- With `cascade=false`, the database foreign key constraint rejects the deletion and the API responds with `409 Conflict`.
- With `cascade=true`, related indicators are removed together with the parent record.
- `GET {id}/dependents` returns the number of dependent indicators and the years they cover. The frontend uses it to show a confirmation modal that states exactly what will be lost before the user confirms.

The safety guarantee lives in the backend, not only in the UI: a client that skips the confirmation modal still cannot delete related data by accident, because `cascade` must be requested explicitly.

### Simulations are not persisted

The simulation mode computes a ranking from the weights supplied in the request and returns the result without writing anything to the database. Stored macro indicator weights are never modified, and the stored data stays limited to real indicator values.

### No authentication

Authentication and user management are intentionally out of scope. The focus of this project is domain logic, architecture and testing, so there is no Identity layer and no user concept.

## Testing

The backend includes unit and integration tests built with xUnit, Moq and FluentAssertions.

- **Unit tests** cover the generic service and the specific services (country, macro indicator, indicator and return rate), with repositories mocked.
- **Integration tests** cover the repositories against a SQLite in-memory database. There is one test class per repository, and each one exercises both the inherited generic methods and the repository's own methods. The suite contains 60 repository tests.

Run all tests from the `backend` folder:

```bash
cd backend
dotnet test
```

A GitHub Actions workflow (`.github/workflows/ci.yml`) restores, builds and tests the backend on every push and pull request that touches the `backend` folder.

## Getting Started

### Prerequisites

- .NET 9 SDK
- Node.js 20 or later and npm
- A Supabase project (PostgreSQL)
- The `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

### Setup

1. Clone the repository

```bash
git clone https://github.com/MintKurisu/IndicVest.git
cd IndicVest
```

2. Copy the example configuration and fill in your values

```bash
cp backend/IndicVestWebApi/appsettings.Example.json \
   backend/IndicVestWebApi/appsettings.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-project.supabase.co;Port=5432;Database=postgres;Username=your-user;Password=your-password"
  }
}
```

3. Apply the migrations

```bash
cd backend
dotnet ef database update --project IndicVest.Infrastructure.Persistence \
  --startup-project IndicVestWebApi
```

4. Run the API

```bash
cd IndicVestWebApi
dotnet run
```

5. Run the frontend

```bash
cd frontend
npm install
npm run dev
```

The API runs on `https://localhost:7129` and Swagger is available at `/swagger`.
The frontend runs on `http://localhost:5173`.

## Notes

- The `appsettings.json` file is excluded from version control. Use `appsettings.Example.json` as a reference.
- The return rate configuration is seeded with default values (0.02 and 0.15) when the database is created.
- The database schema is managed entirely through EF Core migrations.

## Author

Built by [MintKurisu](https://github.com/MintKurisu).
