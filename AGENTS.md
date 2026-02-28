# Repository Guidelines

## Language Requirement
- 所有思考、分析、解释过程与最终输出回答，必须全部使用简体中文。
- 仅当用户明确要求使用其他语言时，才允许切换语言。

## Project Structure & Module Organization
- `backend/`: ASP.NET Core Web API (`net10.0`), controllers in `Controllers/`, EF Core models in `Data/`, domain models in `Models/`, service layer in `Services/`.
- `backend/sql/`: SQL bootstrap/reference scripts (`001_init_schema.sql`, `002_backend_queries.sql`).
- `backend/scripts/`: self-contained publish helpers for Linux/Windows.
- `frontend/`: Vue 3 + TypeScript app, source code under `frontend/src/`.
- `trace-logs/`: session-level change history for audit/troubleshooting.

## Build, Test, and Development Commands
- Backend run: `dotnet run --project backend/GameLibrary.Api.csproj`
- Backend build: `dotnet build backend/GameLibrary.Api.csproj`
- Frontend dev: `cd frontend && npm install && npm run dev`
- Frontend build: `cd frontend && npm run build`
- Frontend type-check: `cd frontend && npm exec -- vue-tsc -b tsconfig.json`
- Self-contained publish (Ubuntu target):  
  `dotnet publish backend/GameLibrary.Api.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o backend/publish/linux-x64`
- Multi-runtime publish scripts:  
  `backend/scripts/publish-self-contained.sh` or `backend/scripts/publish-self-contained.ps1`

## Coding Style & Naming Conventions
- C#: 4-space indentation, `PascalCase` for types/methods, `camelCase` for locals/parameters, async methods end with `Async`.
- Vue/TS: 2-space indentation, `PascalCase` for components, `camelCase` for functions/variables.
- Keep controller logic thin; place persistence/business logic in `Services/`.
- Prefer explicit, descriptive names (`SyncController`, `EfCoreGameLibraryStore`).

## Testing Guidelines
- Current state: no dedicated automated test project yet.
- Minimum before merge: backend compiles, frontend type-check passes, and key APIs smoke-tested (`/api/health`, `/api/library`, `/api/sync/*`).
- When adding tests:
  - Backend: create `tests/` with xUnit naming like `Feature_Action_ExpectedResult`.
  - Frontend: add Vitest-based unit tests for composables/utilities.

## Commit & Pull Request Guidelines
- Use Conventional Commits, e.g. `feat(sync): add platform adapter`, `fix(api): validate steamId`.
- AI-authored commits should include an explicit marker, e.g. `chore(ai): ...`.
- PRs should include:
  - concise summary and scope,
  - config/migration impact (DB, env vars, scripts),
  - verification steps/commands,
  - UI screenshots for frontend-visible changes.

## Security & Configuration Tips
- Never commit real secrets (`Steam API key`, Epic access token, DB password).
- Keep credentials in environment-specific config and mask sensitive values in API responses/logs.
- NLog outputs to console and `backend/logs/`; review logs before production deployment.
