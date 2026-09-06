# Mini.Access.Management

Interview project built with ASP.NET Core 8, Angular 22, PrimeNG 22, EF Core 8,
ASP.NET Core Identity, JWT and MySQL 8.4.

The portal supports user administration, multiple roles, effective permissions,
access requests, ordered two-level approval, provisioning, audit history and a
dashboard. Business table keys use `long`; Identity user and role keys use GUIDs.

## Project structure

```text
backend/src
  Mini.Access.Management.Domain.Shared          enums, role names and validation constants
  Mini.Access.Management.Domain                 one entity class per file
  Mini.Access.Management.Application.Contracts DTOs, service interfaces and paging contracts
  Mini.Access.Management.Application           business services, validation and EF queries
  Mini.Access.Management.EntityFrameworkCore   DbContext, entity mappings, migrations and seed
  Mini.Access.Management.HttpApi               controllers, JWT, Swagger, CORS and middleware
frontend/mini-access-management-ui             Angular standalone application
database                         MySQL and SQL Server interview scripts
```

`EntityFrameworkCore` contains no repositories or business services. Controllers
depend on application-service interfaces. The application layer performs the EF
queries directly to keep this interview solution explicit and easy to follow.

## Prerequisites

- .NET 8 SDK
- Node.js 24 or another Angular 22-supported Node.js release
- Docker Desktop

Check the tools in PowerShell:

```powershell
dotnet --version
node --version
npm --version
docker --version
```

## First-time local setup

1. Create the Docker environment file without committing passwords:

```powershell
Copy-Item .env.example .env
notepad .env
```

2. Start MySQL:

```powershell
docker compose up -d
docker compose ps
```

3. Store API configuration in .NET user-secrets. Use the same app password that
   you placed in `.env` and choose a random signing key of at least 32 characters.

```powershell
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Port=3308;Database=access_management;User=minishop;Password=YOUR_APP_PASSWORD" --project backend/src/Mini.Access.Management.HttpApi
dotnet user-secrets set "Jwt:SigningKey" "YOUR_RANDOM_SIGNING_KEY_AT_LEAST_32_CHARACTERS" --project backend/src/Mini.Access.Management.HttpApi
dotnet user-secrets set "DemoPassword" "Demo@12345" --project backend/src/Mini.Access.Management.HttpApi
```

Do not use or commit credentials copied from the interview screenshots.

4. Restore and run the API. Startup applies EF migrations and idempotently seeds
   demo roles, users, permissions and target systems.

```powershell
dotnet tool restore
dotnet restore Mini.Access.Management.sln
dotnet run --project backend/src/Mini.Access.Management.HttpApi
```

Swagger opens at `http://localhost:5080/swagger`.

5. In a second PowerShell window, run Angular:

```powershell
Set-Location frontend/mini-access-management-ui
npm ci
npm start
```

Open `http://localhost:4200`. Local Angular configuration reads the API address
from `src/environments/environment.local.ts`. Production reads
`src/environments/environment.ts` and defaults to the same host at `/api`.

## Demo users

All demo users use the `DemoPassword` user-secret value.

| Account | Purpose |
|---|---|
| `employee@access.local` | Create and submit a request |
| `manager@access.local` | Level 1 approval |
| `security@access.local` | Level 2 approval |
| `admin@access.local` | User/role administration and provisioning |

## Demonstration flow

1. Sign in as the employee, save a draft and submit it.
2. Sign in as the manager and record the level 1 decision.
3. Sign in as the security approver and record the level 2 decision.
4. Sign in as the admin and provision the approved request.
5. Open Users to see the assigned role and effective permissions.
6. Open Audit Trail to show the actor, action, entity, time and old/new values.

The API prevents duplicate user-role mappings through Identity's composite key,
blocks the Maker/Checker conflict in business logic, and uses database transactions
for multi-step user and workflow operations. Only the current approval level can be
decided. Users are deactivated with a soft delete so historical requests and audit
records remain intact.

`POST /api/users` also requires an `Idempotency-Key` header. The unique
`(Operation, Key)` database index lets a timed-out client safely retry and receive
the user created by the first request instead of creating a second user or audit event.

## EF Core migrations

The initial migration is committed. To create a future migration:

```powershell
$env:ConnectionStrings__Default = "Server=localhost;Port=3308;Database=access_management;User=minishop;Password=YOUR_APP_PASSWORD"
$env:Jwt__SigningKey = "YOUR_RANDOM_SIGNING_KEY_AT_LEAST_32_CHARACTERS"
$env:DemoPassword = "Demo@12345"
dotnet tool run dotnet-ef migrations add YourMigrationName --project backend/src/Mini.Access.Management.EntityFrameworkCore --startup-project backend/src/Mini.Access.Management.HttpApi --output-dir Migrations
dotnet tool run dotnet-ef database update --project backend/src/Mini.Access.Management.EntityFrameworkCore --startup-project backend/src/Mini.Access.Management.HttpApi
```

## Build verification

```powershell
dotnet build Mini.Access.Management.sln
Set-Location frontend/mini-access-management-ui
npm ci
npm run build
```

The SQL answers are in:

- `database/task1_access_management_mysql.sql`
- `database/task2_mssql.sql`
- `database/application_schema_mysql.sql` (generated from the real EF migrations)

Import `postman/AccessManagement.postman_collection.json` to demonstrate the API.
For the full empty-folder setup, project-generation commands, Angular routes,
database migration steps and UI/CSS guide, read `BUILD_ACCESS_PORTAL_FROM_ZERO.md`.

## Assumptions and trade-offs

- Identity owns secure password hashing and framework user/role tables.
- Portal roles and requested business roles share the role table to keep the model
  small enough for an interview exercise.
- MySQL is the application database. The supplied SQL Server questions are answered
  separately in T-SQL.
- JWT access tokens expire after 60 minutes and are stored in session storage.
- Refresh tokens are omitted because the assignment marks them as optional.
- Provisioning is represented by assigning the approved role. A production system
  would normally call an external target-system connector and use an outbox.
