# Command Cheatsheet

## Backend

| Task | Command | Status | Evidence |
| --- | --- | --- | --- |
| Check SDK | `dotnet --info` | Failed during verification | `dotnet` not on PATH. |
| Build API | `dotnet build backend/SupportDesk.Api/SupportDesk.Api.csproj` | Inferred | Project file at `backend/SupportDesk.Api/SupportDesk.Api.csproj:1-8`. |
| Run API | `dotnet run --project backend/SupportDesk.Api` | Inferred | Launch URL at `backend/SupportDesk.Api/Properties/launchSettings.json:4-10`. |
| Test API | `dotnet test` | Not available yet | No test project found in inventory. |

## Frontend

| Task | Command | Status | Evidence |
| --- | --- | --- | --- |
| Check npm | `npm --version` | Passed | Version observed during verification. |
| Install deps | `cd frontend && npm install` | Not run | Original instruction said not to install; deps at `frontend/package.json:11-18`. |
| Dev server | `cd frontend && npm run dev` | Inferred | Script at `frontend/package.json:6-9`. |
| Build | `cd frontend && npm run build` | Not run | Requires deps; script at `frontend/package.json:6-9`. |

## Search And Inspection

| Task | Command | Status |
| --- | --- | --- |
| File inventory | `rg --files` | Passed |
| Find placeholders | `rg "TODO|NotImplemented|console\\.log|debugger|FIXME" support-ticketing-app` | Passed before docs; no product placeholders found |
| Line-number reads | PowerShell `Get-Content | ForEach-Object` | Passed |

## Future Commands After Tooling Exists

| Task | Command |
| --- | --- |
| Add migration | `dotnet ef migrations add InitialSupportDesk` |
| Update DB | `dotnet ef database update` |
| Target tests | `dotnet test --filter FullyQualifiedName~Ticket` |
| Frontend lint | Add script first, then `npm run lint` |
| CI | Add `.github/workflows/ci.yml` after tests/builds are stable |
