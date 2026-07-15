# CatalogAPI

## Overview

The CatalogAPI is responsible for managing games and user purchases.

This microservice stores game information, exposes catalog operations, and handles purchase requests.

The service also subscribes to payment events in order to update user libraries when purchases are approved.

---

## Responsibilities

- Game catalog management
- Game creation
- Game updates
- Game deletion
- Game listing
- Purchase requests
- User library management

---

## Technologies

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- MassTransit

---

## Architecture Role

The CatalogAPI starts the purchase workflow.

When a user purchases a game, the service publishes an event requesting payment processing.

Later, it consumes the payment result event to determine whether the purchased game should be added to the user's library.

---

## Published Events

### OrderPlacedEvent

Published when a purchase request is created.

Example:

```json
{
  "userId": "guid",
  "gameId": "guid",
  "price": 99.90
}
```

---

## Consumed Events

### PaymentProcessedEvent

Received after payment processing.

Example:

```json
{
  "userId": "guid",
  "gameId": "guid",
  "approved": true
}
```

---

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

---

## Main Endpoints

### Create Game

```http
POST /games
```

---

### Update Game

```http
PUT /games/{id}
```

---

### Delete Game

```http
DELETE /games/{id}
```

---

### List Games

```http
GET /games
```

---

### Purchase Game

```http
POST /purchase
```

Publishes an OrderPlacedEvent.

---

## Environment Configuration

```text
ConnectionStrings__DefaultConnection
RabbitMQ__Host
```

---

## Running the Service

### Using Visual Studio

Run the service normally.

### Using CLI

```bash
dotnet restore
dotnet build
dotnet run
```

---

## Swagger

```text
http://localhost:5002/swagger
```

---

## Dependencies

- PostgreSQL
- RabbitMQ

---

## Author

FIAP Tech Challenge – Phase 2
