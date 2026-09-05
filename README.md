# Mini Access Management Portal

Interview project implementing JWT authentication, users, roles, permissions,
access requests, two-level approval, provisioning and audit logging.

## Stack

- ASP.NET Core 10 Web API, Identity, EF Core and MySQL 8.4
- Angular 22 and PrimeNG
- Layered backend with DTO validation, pagination and global exception handling

## Run locally

Requirements: .NET SDK 10, Node.js/npm and Docker.

```bash
export MYSQL_ROOT_PASSWORD="<choose-a-password>"
export MYSQL_PASSWORD="<choose-a-password>"
docker compose up -d
export ConnectionStrings__Default="Server=localhost;Port=3308;Database=access_management;User=minishop;Password=<MYSQL_PASSWORD>"
export Jwt__SigningKey="<at-least-32-random-characters>"
export DemoPassword="<strong-demo-password>"
dotnet run --project backend/src/MiniShop.HttpApi
```

In another terminal:

```bash
cd frontend/minishop-ui
npm ci
npm start
```

Open Angular at `http://localhost:4200`. Swagger is available at the API URL
shown by `dotnet run`.

## Demo accounts

| User | Password | Purpose |
|---|---|---|
| admin@access.local | `DemoPassword` value | Admin and provisioning |
| manager@access.local | `DemoPassword` value | Level 1 approval |
| security@access.local | `DemoPassword` value | Level 2 approval |
| employee@access.local | `DemoPassword` value | Submit requests |

No passwords or signing keys are stored in source control.

## Workflow

1. Employee saves a draft with a target system, role and justification.
2. Submission creates manager approval followed by Security/Admin approval.
3. Approvers act in order; rejection closes the request.
4. Final approval makes the request ready for provisioning.
5. Provisioning assigns the role and records the actor and UTC time.
6. Maker and Checker cannot be assigned to the same user.

Users are soft deleted. Identity's `AspNetUserRoles` composite primary key
prevents duplicate role assignments.

## SQL assignment

- `database/task1_access_management_mysql.sql`: schema, 3 users, 3 roles,
  4 permissions, required queries and indexes.
- `database/task2_mssql.sql`: all T-SQL questions, revoke procedure and index.

## Validate

```bash
dotnet test MiniShop.sln
cd frontend/minishop-ui && npm run build
```

The integration test uses a disposable MySQL Testcontainers database.

## Assumptions and trade-offs

- Identity provides password hashing, users and multi-role mapping.
- Portal roles and requestable roles share one role table to keep the solution small.
- MySQL runs the app; Task 2 is SQL Server T-SQL as requested.
- Refresh tokens are omitted because they are optional.
- `EnsureCreated` keeps setup minimal; a complete SQL schema is included.
