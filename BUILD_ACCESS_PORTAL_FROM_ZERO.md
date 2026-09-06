# Build Mini.Access.Management from zero

This guide recreates the interview project from an empty folder. It explains the
commands and the responsibility of each layer without duplicating every source file.
The finished implementation in this repository is the reference for complete code.

## 1. What is being built

The portal has JWT login, user management, many-to-many roles and permissions,
access-request drafts, two ordered approval levels, provisioning, audit history and
an admin dashboard. MySQL is the application database. The separate SQL Server file
answers the T-SQL portion of the interview.

```text
Angular component -> API controller -> application-service interface
                  -> application service -> EF DbContext -> MySQL
```

Dependency direction:

```text
Domain.Shared <- Domain <- EntityFrameworkCore
       ^           ^              ^
       |           |              |
Application.Contracts <- Application <- HttpApi
                                  ^       ^
                                  +-------+
```

- `Mini.Access.Management.Domain.Shared`: enums, fixed portal role names and validation constants.
- `Mini.Access.Management.Domain`: one entity per file and no database code.
- `Mini.Access.Management.Application.Contracts`: DTOs and service interfaces used by controllers.
- `Mini.Access.Management.Application`: business rules and explicit EF Core queries.
- `Mini.Access.Management.EntityFrameworkCore`: DbContext, configurations, migrations and seeding.
- `Mini.Access.Management.HttpApi`: controllers, JWT, CORS, Swagger and exception middleware.

Business records use `long` keys. ASP.NET Core Identity users and roles use `Guid`
keys because Identity owns those framework tables.

## 2. Prerequisites

Install .NET 8 SDK, Node.js supported by Angular 22, Git and Docker Desktop. Check
them in Windows PowerShell:

```powershell
dotnet --version
node --version
npm --version
git --version
docker --version
```

## 3. Create the backend solution

```powershell
New-Item -ItemType Directory access-management
Set-Location access-management
dotnet new globaljson --sdk-version 8.0.319 --roll-forward latestPatch
dotnet new sln -n Mini.Access.Management
New-Item -ItemType Directory backend/src -Force

dotnet new classlib -n Mini.Access.Management.Domain.Shared -f net8.0 -o backend/src/Mini.Access.Management.Domain.Shared
dotnet new classlib -n Mini.Access.Management.Domain -f net8.0 -o backend/src/Mini.Access.Management.Domain
dotnet new classlib -n Mini.Access.Management.Application.Contracts -f net8.0 -o backend/src/Mini.Access.Management.Application.Contracts
dotnet new classlib -n Mini.Access.Management.Application -f net8.0 -o backend/src/Mini.Access.Management.Application
dotnet new classlib -n Mini.Access.Management.EntityFrameworkCore -f net8.0 -o backend/src/Mini.Access.Management.EntityFrameworkCore
dotnet new webapi -n Mini.Access.Management.HttpApi -f net8.0 --use-controllers -o backend/src/Mini.Access.Management.HttpApi

dotnet sln Mini.Access.Management.sln add (Get-ChildItem backend/src -Filter *.csproj -Recurse).FullName
```

Add project references in the same direction as the diagram:

```powershell
dotnet add backend/src/Mini.Access.Management.Domain/Mini.Access.Management.Domain.csproj reference backend/src/Mini.Access.Management.Domain.Shared/Mini.Access.Management.Domain.Shared.csproj
dotnet add backend/src/Mini.Access.Management.Application.Contracts/Mini.Access.Management.Application.Contracts.csproj reference backend/src/Mini.Access.Management.Domain.Shared/Mini.Access.Management.Domain.Shared.csproj
dotnet add backend/src/Mini.Access.Management.Application/Mini.Access.Management.Application.csproj reference backend/src/Mini.Access.Management.Application.Contracts/Mini.Access.Management.Application.Contracts.csproj backend/src/Mini.Access.Management.Domain/Mini.Access.Management.Domain.csproj backend/src/Mini.Access.Management.EntityFrameworkCore/Mini.Access.Management.EntityFrameworkCore.csproj
dotnet add backend/src/Mini.Access.Management.EntityFrameworkCore/Mini.Access.Management.EntityFrameworkCore.csproj reference backend/src/Mini.Access.Management.Domain/Mini.Access.Management.Domain.csproj
dotnet add backend/src/Mini.Access.Management.HttpApi/Mini.Access.Management.HttpApi.csproj reference backend/src/Mini.Access.Management.Application/Mini.Access.Management.Application.csproj backend/src/Mini.Access.Management.EntityFrameworkCore/Mini.Access.Management.EntityFrameworkCore.csproj
```

The Application project references EntityFrameworkCore because this interview uses
the concrete DbContext directly in business services. This keeps repositories and
extra abstractions out of the solution, as requested.

Install the packages pinned by this repository:

```powershell
dotnet add backend/src/Mini.Access.Management.EntityFrameworkCore package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.30
dotnet add backend/src/Mini.Access.Management.EntityFrameworkCore package Microsoft.EntityFrameworkCore.Design --version 8.0.30
dotnet add backend/src/Mini.Access.Management.EntityFrameworkCore package MySql.EntityFrameworkCore --version 8.0.28
dotnet add backend/src/Mini.Access.Management.HttpApi package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.30
dotnet add backend/src/Mini.Access.Management.HttpApi package Microsoft.EntityFrameworkCore.Design --version 8.0.30
dotnet add backend/src/Mini.Access.Management.HttpApi package Swashbuckle.AspNetCore --version 10.2.3
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 8.0.30
```

Oracle's `MySql.EntityFrameworkCore` provider is used with EF Core 8. Keep every
Microsoft EF/ASP.NET package on the same patch version.

## 4. Create backend folders and files

Create feature folders, not large combined files:

```powershell
$folders = @(
  'backend/src/Mini.Access.Management.Domain/AccessRequests',
  'backend/src/Mini.Access.Management.Domain/Approvals',
  'backend/src/Mini.Access.Management.Domain/Auditing',
  'backend/src/Mini.Access.Management.Domain/Idempotency',
  'backend/src/Mini.Access.Management.Domain/Identity',
  'backend/src/Mini.Access.Management.Domain/Permissions',
  'backend/src/Mini.Access.Management.Domain/Systems',
  'backend/src/Mini.Access.Management.Application.Contracts/AccessRequests',
  'backend/src/Mini.Access.Management.Application.Contracts/Auth',
  'backend/src/Mini.Access.Management.Application.Contracts/Roles',
  'backend/src/Mini.Access.Management.Application.Contracts/Users',
  'backend/src/Mini.Access.Management.Application/AccessManagement',
  'backend/src/Mini.Access.Management.Application/Auth',
  'backend/src/Mini.Access.Management.EntityFrameworkCore/Configurations',
  'backend/src/Mini.Access.Management.HttpApi/Controllers'
)
$folders | ForEach-Object { New-Item -ItemType Directory $_ -Force }
```

Use the repository files in those folders as the implementation reference. Important
model rules are:

- Identity `ApplicationUser` supports manager, active state and soft delete.
- Identity already supplies the unique `(UserId, RoleId)` user-role key.
- `RolePermission` has a unique composite key.
- `AccessRequest` links requester, target system and requested role.
- `ApprovalHistory` has a unique `(AccessRequestId, ApprovalLevel)` key.
- `AccessRequest.Version` is an EF concurrency token.
- `IdempotencyRecord` uniquely stores operation/key, request hash and original response.
- `AuditLog` records actor, action, entity, time and old/new JSON values.

The entity classes use normal C# nullability, so EF already knows which string fields
are required. Only strings used by MySQL indexes have a small `[MaxLength]` attribute;
unlimited `longtext` columns cannot be used as index keys. Keep the EF configuration
classes focused on relationships, indexes, delete behavior and concurrency.

Apply `IEntityTypeConfiguration<T>` classes from `Configurations` inside
`AccessManagementDbContext.OnModelCreating` with:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccessManagementDbContext).Assembly);
```

The configuration classes are not called manually, so an editor may show zero direct
references. EF discovers them by assembly scanning at runtime.

In `Application.Contracts`, keep `IAuthAppService`, `IUserAppService`,
`IRolePermissionAppService`, `IAccessRequestAppService`, `IAuditLogAppService` and
`IDashboardAppService`. Controllers inject these interfaces. Their implementations
belong in Application. Do not add a generic `Abstractions` or `Common` folder.

## 5. Configure MySQL and secrets

Create `.env.example`:

```dotenv
MYSQL_ROOT_PASSWORD=change-this-root-password
MYSQL_PASSWORD=change-this-app-password
```

Copy it locally, then edit the local values. `.env` must stay in `.gitignore`.

```powershell
Copy-Item .env.example .env
notepad .env
docker compose up -d
docker compose ps
```

Use the repository `docker-compose.yml`: it starts MySQL 8.4 on local port 3308 and
creates the `access_management` database/user.

Initialize and set API user-secrets. Never put the JWT signing key in appsettings:

```powershell
dotnet user-secrets init --project backend/src/Mini.Access.Management.HttpApi
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Port=3308;Database=access_management;User=minishop;Password=YOUR_APP_PASSWORD" --project backend/src/Mini.Access.Management.HttpApi
dotnet user-secrets set "Jwt:SigningKey" "YOUR_RANDOM_KEY_WITH_AT_LEAST_32_CHARACTERS" --project backend/src/Mini.Access.Management.HttpApi
dotnet user-secrets set "DemoPassword" "Demo@12345" --project backend/src/Mini.Access.Management.HttpApi
```

## 6. Create and apply EF migrations

```powershell
dotnet restore Mini.Access.Management.sln
dotnet tool run dotnet-ef migrations add InitialAccessManagement --project backend/src/Mini.Access.Management.EntityFrameworkCore --startup-project backend/src/Mini.Access.Management.HttpApi --output-dir Migrations
dotnet tool run dotnet-ef database update --project backend/src/Mini.Access.Management.EntityFrameworkCore --startup-project backend/src/Mini.Access.Management.HttpApi
dotnet tool run dotnet-ef migrations script --idempotent --output database/application_schema_mysql.sql --project backend/src/Mini.Access.Management.EntityFrameworkCore --startup-project backend/src/Mini.Access.Management.HttpApi
```

For later model changes, replace the migration name and repeat the last two commands.
The API also calls the seeder during development startup. Seeding is idempotent, so
roles, users, permissions and systems are not duplicated.

## 7. Configure the API host

In `Program.cs`, register controllers, Swagger bearer authentication, CORS for
`http://localhost:4200`, Application services, EntityFrameworkCore/Identity and JWT.
The middleware order is exception middleware, Swagger, CORS, authentication,
authorization and controller mapping.

Application services use standard .NET exceptions only: `ArgumentException`,
`InvalidOperationException`, `KeyNotFoundException`, `UnauthorizedAccessException`
and `SecurityException`. `ApiExceptionMiddleware` translates them into HTTP responses.

The API uses consistent HTTP behavior:

- 400: model or business validation failure.
- 401: missing/invalid token or invalid login.
- 403: authenticated user lacks the required role.
- 404: requested record does not exist.
- 409: uniqueness, state-transition, idempotency or concurrency conflict.

Create-user accepts `Idempotency-Key`. The record, user, initial audit entry and role
work occur in one transaction. A retry with the same body replays the stored response;
reusing the key with a different body returns a conflict.

## 8. Create Angular 22 and PrimeNG 22

```powershell
New-Item -ItemType Directory frontend -Force
Set-Location frontend
npx @angular/cli@22.1.7 new mini-access-management-ui --standalone --routing --style=css --skip-git --package-manager=npm
Set-Location mini-access-management-ui
npm install primeng@22.1.0 @primeuix/themes@3 primeicons@8 @angular/cdk@22.1.5 @angular/animations@22.1.5
```

Generate the pages and core services:

```powershell
ng generate component features/auth/login --standalone --skip-tests
ng generate component features/dashboard --standalone --skip-tests
ng generate component features/users --standalone --skip-tests
ng generate component features/roles --standalone --skip-tests
ng generate component features/requests --standalone --skip-tests
ng generate component features/approvals --standalone --skip-tests
ng generate component features/provisioning --standalone --skip-tests
ng generate component features/audit --standalone --skip-tests
ng generate service core/auth --skip-tests
ng generate service core/access-api --skip-tests
ng generate environments
```

This repository uses standalone components and lazy `loadComponent` routes. Add
functional `authGuard`, `adminGuard`, `approverGuard` and `provisionerGuard`, plus a
functional HTTP interceptor that adds `Authorization: Bearer <token>`. Store the JWT
in `sessionStorage`, restore `/api/auth/me` on refresh and route each role to its first
authorized page.

Use typed reactive forms on every editable page. Approval remarks are required and
must contain 3-500 characters. Listing endpoints send `page`, `pageSize` and optional
`search`; PrimeNG pagers use the total returned by the API.

## 9. Local API URL environment

Create `src/environments/environment.ts`:

```typescript
export const environment = { apiBaseUrl: '/api' };
```

Create `src/environments/environment.local.ts`:

```typescript
export const environment = { apiBaseUrl: 'http://localhost:5080/api' };
```

In `angular.json`, configure the development file replacement from `environment.ts`
to `environment.local.ts`. The API service reads only `environment.apiBaseUrl`; do
not hard-code API URLs inside components.

## 10. PrimeNG and minimal CSS guide

Configure PrimeNG in `app.config.ts` with `providePrimeNG` and the Aura preset. Import
`primeicons/primeicons.css` globally. This project uses PrimeNG for buttons, tables,
dialogs, selects, inputs, tags, toolbar, toast and confirmation dialog.

No Bootstrap or Tailwind is used. PrimeNG supplies the component styling, while the
small `styles.css` file handles only application layout: page width, navigation,
forms, cards, responsive grids and horizontal table overflow. Keep class names only
when an HTML element needs one of these real layout rules. Remove a class when its CSS
rule is removed, and search both directions before finishing:

```powershell
rg -o 'class="[^"]+"' src/app -g '*.html'
rg -n '^\.[A-Za-z_-]' src/styles.css
```

For responsive pages:

- wrap wide tables in an overflow container;
- use CSS grid with `minmax(0, 1fr)` for form columns;
- stack form fields below the mobile breakpoint;
- keep action buttons wrapping instead of forcing page-wide scrolling;
- add a visible close button to the mobile navigation drawer.

## 11. Run the projects

PowerShell window 1:

```powershell
dotnet run --project backend/src/Mini.Access.Management.HttpApi
```

PowerShell window 2:

```powershell
Set-Location frontend/mini-access-management-ui
npm ci
npm start
```

Open `http://localhost:5080/swagger` and `http://localhost:4200`.

## 12. Demonstrate the workflow

1. Log in as employee and create a draft for a target system/requestable role.
2. Submit the draft; its status becomes Pending.
3. Log in as manager and enter a required approval remark.
4. Log in as security and approve level 2.
5. Log in as admin and provision the request.
6. Confirm the role appears on the user and the provisioner/time is displayed.
7. Open Audit Trail and show create, submit, approval and provisioning records.
8. Retry a create-user request with the same `Idempotency-Key` and body.
9. Reuse that key with a changed body and explain the 409 response.

Import `postman/AccessManagement.postman_collection.json`, run Login first, and then
run the numbered requests. Switch `adminEmail` when demonstrating employee/manager/
security authorization.

## 13. Database interview answers

- `database/task1_access_management_mysql.sql` is the standalone normalized MySQL
  schema, sample data, effective-permission query, pending-approval query,
  Maker/Checker conflict query and explained indexes.
- `database/task2_mssql.sql` answers all supplied T-SQL queries, uses an atomic stored
  procedure with `XACT_ABORT`, and includes query-1/query-3 indexes.
- `database/application_schema_mysql.sql` is the deployable script generated from EF.

Do not run Task 1's standalone script against the application's Identity database;
they intentionally model users differently.

## 14. Common problems

- `Jwt:SigningKey must be at least 32 bytes`: set the user-secret for HttpApi.
- MySQL access denied: ensure `.env` and the connection-string secret use the same
  password. Existing Docker volumes retain the password from their first creation.
- EF cannot create DbContext: use both `--project` and `--startup-project` exactly as
  shown and ensure secrets are configured.
- Angular 401: log in again because session storage is per browser tab/session.
- Angular 403: the token is valid, but the account lacks the route/API role.
- Migration file lock: stop the running API before building or generating a migration.
- CORS error: run Angular at localhost:4200 or update the named CORS policy.

## 15. Final checklist

```powershell
dotnet build Mini.Access.Management.sln --no-restore
Set-Location frontend/mini-access-management-ui
npm run build
```

- All list APIs return `PagedResult<T>` and enforce a maximum page size.
- Controllers depend on Contracts interfaces.
- Application owns business/EF query logic; EntityFrameworkCore owns persistence setup.
- Soft deletion, uniqueness, transactions, idempotency and concurrency are enforced.
- JWT guards and server authorization both protect privileged features.
- UI has real responsive rules and no decorative fake classes.
- Swagger, Postman, SQL scripts, README and this recreation guide are present.
