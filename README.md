# CatalogAPI

## Overview
The **CatalogAPI** is responsible for managing the game catalog, intelligent search, distributed caching, user game reviews, and user purchases.

This microservice stores core transactional game data in PostgreSQL, manages high-volume user reviews in Amazon DynamoDB, accelerates read operations with Redis, and provides typo-tolerant search capabilities via Elasticsearch.

The service also subscribes to payment events in order to update user libraries when purchases are approved.

## Responsibilities
- Game catalog management (creation, updates, deletion, and listing)
- Distributed read caching with Redis (Cache-Aside pattern)
- Full-text and Fuzzy Search via Elasticsearch with automatic index synchronization
- Game reviews management stored in Amazon DynamoDB (`AWSSDK.DynamoDBv2`)
- Purchase requests and user library updates via asynchronous messaging

## Technologies
- **.NET 8** / ASP.NET Core Web API
- **Entity Framework Core** & **PostgreSQL** (Relational transactional database)
- **Amazon DynamoDB** (`AWSSDK.DynamoDBv2` — NoSQL database for game reviews)
- **Redis** (`StackExchange.Redis` — Distributed cache)
- **Elasticsearch** (Dedicated search engine with Fuzzy Search and relevance scoring)
- **RabbitMQ** & **MassTransit** (Event-driven messaging)
- **Kubernetes (K3s)** & **GitHub Actions** (CI/CD with Amazon ECR)

## Architecture Role & Polyglot Persistence
The **CatalogAPI** adopts a **Polyglot Persistence** strategy and initiates the purchase workflow:

1. **PostgreSQL (Primary Transactional Store):** Stores games, pricing, and user libraries with ACID guarantees.
2. **Elasticsearch (Dedicated Search Engine):** Decouples search queries from the relational database. Whenever a game is created or updated in PostgreSQL, the Elasticsearch index is automatically synchronized.
3. **Redis (Distributed Cache):** Implements the **Cache-Aside** pattern on catalog read endpoints (`FCG_Catalog_*` keys) to reduce database load and response latency.
4. **Amazon DynamoDB (NoSQL Store):** Persists high-volume user reviews per game (`Reviews`) using `AWSSDK.DynamoDBv2`.
5. **Event-Driven Purchase Workflow:** Publishes `OrderPlacedEvent` upon purchase and consumes `PaymentProcessedEvent` to grant game access in the user's library.

## Published Events
### `OrderPlacedEvent`
Published when a purchase request is created.

Example:
```json
{
  "userId": "guid",
  "gameId": "guid"
}

## Consumed Events

### `PaymentProcessedEvent`
Received after payment processing.

Example:
```json
{
  "userId": "guid",
  "gameId": "guid",
  "approved": true
}

## Purchase Workflow
```text
CatalogAPI
    │
    ▼
OrderPlacedEvent
    │
    ▼
PaymentsAPI
    │
    ▼
PaymentProcessedEvent
    │
    ▼
CatalogAPI
```

## Main Endpoints

### Catalog & Cache (PostgreSQL + Redis + Elasticsearch Sync)
- **Create Game** (saves to PostgreSQL and indexes in Elasticsearch): `POST /games`
- **Update Game** (updates PostgreSQL, invalidates cache, and updates Elasticsearch index): `PUT /games/{id}`
- **Delete Game**: `DELETE /games/{id}`
- **List Games** (cached with Redis): `GET /games`
- **Get Game by ID** (cached with Redis): `GET /games/{id}`

### Search (Elasticsearch)
- **Fuzzy Search & Relevance Ranking**: `GET /games/search?term={query}`
  - Supports typo tolerance (Fuzzy Search) and orders results by relevance score independently from PostgreSQL.

### Reviews (Amazon DynamoDB)
- **Create Game Review**: `POST /games/{id}/reviews`
- **List Game Reviews**: `GET /games/{id}/reviews`

### Purchases & Library
- **Purchase Game** (publishes `OrderPlacedEvent`): `POST /purchase`
- **User Library**: `GET /library` / `GET /library/{userId}`

## Environment Configuration (Zero Hardcoded Credentials)
All sensitive credentials and service endpoints are injected at runtime via environment variables (Kubernetes `Secrets` and `ConfigMaps`):

- `ConnectionStrings__DefaultConnection`
- `Redis__ConnectionString`
- `Elasticsearch__Uri`
- `AWS__Region`
- `AWS__DynamoDB__ReviewsTableName`
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `AWS_SESSION_TOKEN`
- `RabbitMQ__Host`
- `RabbitMQ__Username`
- `RabbitMQ__Password`
- `Jwt__Key`

## Running the Service

### Using Visual Studio
Run the service normally using the `http` or `https` launch profile.

### Using CLI
```bash
dotnet restore
dotnet build
dotnet run
```

### Swagger UI
- **Local:** `http://localhost:5002/swagger`
- **Kubernetes Cluster (via Ingress):** `http://<EC2_PUBLIC_IP>/catalog/swagger/index.html`

## Dependencies
- PostgreSQL
- Amazon DynamoDB
- Redis
- Elasticsearch
- RabbitMQ
