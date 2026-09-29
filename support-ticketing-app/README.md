# Customer Support Ticketing App

A learning-oriented Zendesk/Intercom-style inbox built with C#, ASP.NET Core, SignalR, and React.

This repo is intentionally small enough to read, but shaped like a real codebase:

- Tickets with comments, internal notes, assignments, tags, priorities, statuses, and SLA timers
- CQRS-ish application layer with command/query handlers
- SignalR hub for live ticket updates
- Search and filtering across status, assignee, tags, priority, SLA state, and text
- Optimistic concurrency via ticket versions and `expectedVersion`
- Request-header based user context for learning permissions without a full identity provider
- React inbox UI with filters, ticket detail, comments, notes, canned replies, and live updates

## Layout

```text
support-ticketing-app/
  backend/SupportDesk.Api/      ASP.NET Core API + SignalR
  frontend/                     React + Vite client
```

## Run

This machine did not have the .NET SDK available when the code was generated, so the project was not compiled here.

When you have the SDK installed:

```powershell
cd support-ticketing-app/backend/SupportDesk.Api
dotnet run
```

In another terminal:

```powershell
cd support-ticketing-app/frontend
npm install
npm run dev
```

The API listens on `http://localhost:5208` by default and the frontend expects that URL.

## Demo Users

The API reads user context from headers. The frontend sends these demo headers:

- `X-User-Id`: `agent-1`
- `X-User-Name`: `Avery Stone`
- `X-User-Role`: `Agent`
- `X-Team-Ids`: `billing,vip`

Try changing them in `frontend/src/api.ts` to see the permission behavior.

Roles:

- `Admin`: can do everything
- `Manager`: can assign, status-change, tag, and add internal notes
- `Agent`: can work assigned/team tickets and add internal notes
- `Customer`: can create tickets and add public comments

## API Highlights

```http
GET    /api/tickets?status=Open&query=refund&tag=billing
GET    /api/tickets/{id}
POST   /api/tickets
POST   /api/tickets/{id}/comments
PUT    /api/tickets/{id}/assignment
PUT    /api/tickets/{id}/status
PUT    /api/tickets/{id}/tags
GET    /api/canned-replies
```

All mutation endpoints return the updated ticket and require `expectedVersion` where concurrency matters.

## SignalR

Clients connect to:

```text
/hubs/tickets
```

The server broadcasts:

- `ticket.created`
- `ticket.updated`
- `ticket.commented`

Each payload contains a compact ticket summary suitable for updating inbox lists.


## Ticket engineering course

Read the [detailed CRUD learning course](../astraupskill/README.md) for safe mutations, permissions, pagination, live refreshes, complete test examples and practice solutions.
