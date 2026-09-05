# JoyStore Backend

Backend for a **Telegram Mini App marketplace** that sells digital game keys. Built as a set of
**.NET microservices** behind an API gateway, with a background parser that keeps the catalog
up to date and Redis caching on the hot paths.

## What it does

- **Catalog, cart and orders** — core marketplace flow for browsing and buying digital keys.
- **Payments** — payment gateway integrated and adapted to the Telegram Mini App environment for
  smooth purchases of digital keys.
- **Automated catalog** — a background parser refreshes prices, descriptions and metadata from
  external sources daily, keeping the catalog current with no manual work.
- **Admin panel** — CRUD over products plus basic statistics, so the catalog and promotions can
  be managed on the fly.

## Architecture

The system is split into independent services communicating through a gateway:

```
Client (Telegram Mini App)
        │
        ▼
  Gateway.WebApi        # single entry point / routing
        │
        ├──▶ Auth.WebApi        # authentication & authorization
        ├──▶ Service(s)         # catalog, cart, orders, admin, analytics
        ├──▶ ParseService       # background catalog parser (prices, metadata)
        └──▶ CacheService       # Redis caching layer
                │
                ▼
          PostgreSQL            # primary data store
```
## Tech Stack

- **Language / Platform:** C#, .NET 6
- **Database:** PostgreSQL
- **Caching:** Redis
- **Containerization:** Docker, Docker Compose
- **CI/CD:** GitHub Actions

## Project layout

```
joyStore_backend/
├── Gateway.WebApi/       # API gateway — single entry point
├── Auth.WebApi/          # authentication service
├── ParseService/         # background catalog parser
├── CacheService/         # Redis caching
├── Service.Application/  # application/use-case layer
├── Service/ , Services/  # service implementations
├── Business.Data/        # business & data models
├── DataBaseToAccess/     # data access
├── Enum/ , Region.cs     # shared domain types
├── CalculationPrice.cs   # price calculation logic
└── docker-compose.yml    # local orchestration
```

## Run locally

```bash
docker compose up --build
```

The gateway will be available on the port configured in `docker-compose.yml`.
