# DStack Event Store Services

This project uses both EventStore and Kurrent for event sourcing capabilities.

## Quick Start

```bash
# Start all services (EventStore + Kurrent)
docker-compose -f eventstore-stack.yml up -d

# Check services are running
docker-compose -f eventstore-stack.yml ps

# Stop all services
docker-compose -f eventstore-stack.yml down
```

## Services

### EventStore (Original)
- **Port:** 2113
- **Web UI:** http://localhost:2113
- **Connection String:** `esdb://localhost:2113?Tls=false`
- **Used by:** DStack.Aggregates.EventStoreDB

### Kurrent (Rebranded EventStore)
- **Port:** 2114
- **Web UI:** http://localhost:2114
- **Connection String:** `esdb://localhost:2114?Tls=false`
- **Used by:** DStack.Aggregates.KurrentDB

Both services share the same network and can run simultaneously without port conflicts.

## Running Individual Services

```bash
# Start only EventStore
docker-compose -f eventstore-stack.yml up -d eventstore

# Start only Kurrent
docker-compose -f eventstore-stack.yml up -d kurrentdb
```

## Running Tests

### EventStore Integration Tests
```bash
# Ensure EventStore is running
docker-compose -f eventstore-stack.yml up -d eventstore

# Run tests
dotnet test DStack.Aggregates.EventStoreDB.IntegrationTests
```

### Kurrent Integration Tests
```bash
# Ensure Kurrent is running
docker-compose -f eventstore-stack.yml up -d kurrentdb

# Run tests
dotnet test DStack.Aggregates.KurrentDB.IntegrationTests
```

## Troubleshooting

### View Logs
```bash
# All services
docker-compose -f eventstore-stack.yml logs -f

# Specific service
docker-compose -f eventstore-stack.yml logs -f eventstore
docker-compose -f eventstore-stack.yml logs -f kurrentdb
```

### Reset Data
```bash
# Stop services and remove volumes
docker-compose -f eventstore-stack.yml down -v
```

### Check Port Usage
```powershell
# Windows PowerShell
netstat -an | findstr "2113 2114"
```

## Architecture Notes

- **Kurrent** is EventStore rebranded with updated client libraries
- Both implementations maintain API compatibility
- Tests verify feature parity between implementations
- Separate ports allow testing both simultaneously
