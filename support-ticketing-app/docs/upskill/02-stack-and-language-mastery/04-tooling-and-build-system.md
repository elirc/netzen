# 04 Tooling And Build System

## Backend Build

| Topic | Evidence | Command |
| --- | --- | --- |
| Web SDK | `backend/SupportDesk.Api/SupportDesk.Api.csproj:1` | `dotnet build backend/SupportDesk.Api/SupportDesk.Api.csproj` |
| Target framework | `backend/SupportDesk.Api/SupportDesk.Api.csproj:2-7` | Requires .NET 8 SDK. |
| Local run URL | `backend/SupportDesk.Api/Properties/launchSettings.json:4-10` | `dotnet run --project backend/SupportDesk.Api` |

Status: inferred. `dotnet` was not available on PATH during verification.

## Frontend Build

| Topic | Evidence | Command |
| --- | --- | --- |
| Dev server | `frontend/package.json:6-9` | `npm run dev` |
| Production build | `frontend/package.json:6-9` | `npm run build` |
| Dependencies | `frontend/package.json:11-18` | `npm install` first. |
| Strict TS | `frontend/tsconfig.json:1-22` | `npm run build` runs `tsc -b`. |

Status: npm was available, but install/build were not run.

## Package Management

No lockfile was found in the repo inventory. That means dependency resolution can drift between machines. Evidence: `frontend/package.json:11-18` declares semver ranges; there is no `package-lock.json` in `rg --files` output recorded in `08-reference/verification-log.md`.

No central NuGet management is present. Evidence: only one `.csproj` exists and it has no package references at `backend/SupportDesk.Api/SupportDesk.Api.csproj:1-8`.

## Tooling Drills

1. Run `dotnet --info` and record SDK version in `08-reference/verification-log.md`.
2. Run `dotnet build backend/SupportDesk.Api/SupportDesk.Api.csproj`; if it fails, capture the first compiler error and map it to a file.
3. Run `npm install` only when allowed, then `npm run build`; compare TS errors to `frontend/src/types.ts:1-63`.
4. Add a lockfile in a real contribution only with reviewer agreement because it changes dependency reproducibility for everyone.
