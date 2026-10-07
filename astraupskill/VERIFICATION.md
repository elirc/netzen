# 9. Verification

Run the portable checks from the project root [`../`](../). They use the test project and frontend package lock that are actually present in [`../support-ticketing-app/`](../support-ticketing-app/).

```powershell
dotnet restore support-ticketing-app/backend/SupportDesk.Api.Tests/SupportDesk.Api.Tests.csproj
$env:DOTNET_ROLL_FORWARD = 'Major'
dotnet test support-ticketing-app/backend/SupportDesk.Api.Tests/SupportDesk.Api.Tests.csproj -c Release --no-restore
Push-Location support-ticketing-app/frontend
npm ci
npm run build
Pop-Location
```

The restore targets .NET 8 project [`SupportDesk.Api.Tests.csproj`](../support-ticketing-app/backend/SupportDesk.Api.Tests/SupportDesk.Api.Tests.csproj). On a machine with .NET 10 runtime and no native .NET 8 runtime, `DOTNET_ROLL_FORWARD=Major` allows the target to run on the installed major runtime. The recorded backend evidence is `root-dotnet-test-01` (2026-09-12): 3 xUnit tests pass with TRX ([`ticket-safety-tests.trx`](evidence/ticket-safety-tests.trx) shows `total="3" passed="3"`). The fourth test, `Create_RejectsUndefinedPriority`, was added afterwards; see the fix-pass section below. The original three prove that `ReplaceTags` materializes before mutation and leaves tags/version unchanged when enumeration fails; that `ChangeStatus`, `UpdatePriority`, and `AddComment` reject undefined enum values; and that a 121-ticket fixture returns the customer-owned `T-000001` across source pages while an internal-only marker returns no customer results.

The frontend command runs `npm ci` in [`frontend`](../support-ticketing-app/frontend/), using the included `package-lock.json`, then `npm run build`. The recorded `root-web-build-02` result passes after the React type declarations and `vite-env.d.ts` support the current TypeScript/Vite source. This build proves compilation and bundling. It does not mount `App`, open a SignalR connection, assert browser DOM behavior, or run a live HTTP test server. Do not invent a frontend unit-test pass from the build.

The implementation's other limits belong in the evidence note. There is no database, durable outbox, live hub, browser, external account, deployment, or real authentication. The `X-User-Id`, `X-User-Role`, and `X-Team-Ids` headers in [`api.ts`](../support-ticketing-app/frontend/src/api.ts) are demo identity. The all-pages search is appropriate for the small in-memory store but is not an atomic snapshot across writes. Future pagination should push authorization into the query. The sample does not prove complete actor policies or durable notification delivery. (The creation-time priority guard it once lacked was added and tested on 2026-09-17.)

Committed evidence is the [results summary](evidence/results.json), the [TRX report](evidence/ticket-safety-tests.trx), and the per-check records `evidence/root-dotnet-test-01.json`, `root-web-types-01.json` and `root-web-build-02.json`. Those records name `.log` files that were never committed to this repo, so there is no terminal output to read for the 2026-09-12 runs. When you run the suite yourself, verify that it discovers **four** cases rather than assuming an empty successful command ran tests.

Search text currently excludes internal comments for every actor, including agents. This avoids the customer marker leak but makes staff internal-note search unavailable. Browser request counters protect list refreshes and hub-triggered detail refreshes; the existing user-driven `openTicket` and mutation completion paths do not share a request coordinator, and finding F1 in [06](06-SOLUTIONS-AND-REVIEW.md) traces a concrete race that follows from that. Tests have not established behavior for every selection or mutation race. Capture those cases before extending the UI so a locally correct change does not claim broader guarantees.

**Exercise 8.** Draft a verification note for the recorded run. Include `root-dotnet-test-01`, 3 xUnit tests with TRX (and the later 4/4 fix-pass run), `root-web-build-02`, the .NET roll-forward setting, and the fact that no live hub or browser was tested. State one conclusion the evidence supports and one it cannot support.

## Fix-pass re-run (2026-09-17)

Two follow-ups from the review were applied: `Enum.IsDefined(priority)` was added to `Ticket.Create` (it had been added to the three mutators but not to the factory), and the React hub effect was changed to one connection per mount with `filtersRef`/`selectedIdRef`.

```
$env:DOTNET_ROLL_FORWARD = "Major"
dotnet test support-ticketing-app/backend/SupportDesk.Api.Tests/SupportDesk.Api.Tests.csproj -c Release
Passed!  - Failed:     0, Passed:     4, Skipped:     0, Total:     4, Duration: 1 s - SupportDesk.Api.Tests.dll (net8.0)
```

Three of those four cases are the delivered ones; the fourth is `Create_RejectsUndefinedPriority`. Without `DOTNET_ROLL_FORWARD=Major` the run aborts on this machine, because only the .NET 10 runtime is installed while the projects target `net8.0`.

The frontend change was checked with `npx tsc --noEmit -p tsconfig.json` in `support-ticketing-app/frontend` (exit code 0) against a copied `node_modules`; `node_modules` was then removed again, as the delivered tree does not contain one. **There is still no frontend test**: the stale-list, stale-detail and draft-preservation behaviours remain source-reviewed only, and the project has no test runner (no vitest, no Testing Library) to host one.
