# DStack.Aggregates.KurrentDB Integration Tests

This project contains integration tests for the KurrentDB aggregate repository implementation.

## Prerequisites

To run these tests, you need:

1. **Kurrent (EventStoreDB) instance running locally**
   - The tests expect KurrentDB to be available at `localhost:2114`
   - KurrentDB runs on port 2114 to avoid collision with EventStore on port 2113

2. **Configuration**
   - The connection string is configured in `appsettings.json`
   - Default: `esdb://localhost:2114?Tls=false`
   - Modify this if your Kurrent instance runs on a different host/port

## Running Kurrent

### Option 1: Using Docker Compose (Recommended)

The project includes a `eventstore-stack.yml` file that runs both EventStore and Kurrent:

```bash
# Start both EventStore (port 2113) and Kurrent (port 2114)
docker-compose -f eventstore-stack.yml up -d

# Or start only Kurrent
docker-compose -f eventstore-stack.yml up -d kurrentdb

# Stop services
docker-compose -f eventstore-stack.yml down

# Stop services and remove volumes
docker-compose -f eventstore-stack.yml down -v
```

**Port Mapping:**
- EventStore: `localhost:2113`
- Kurrent: `localhost:2114` (mapped from container's internal port 2113)

### Option 2: Using Standalone Docker

If you only want to run Kurrent without the full stack:

```bash
docker run -d --name kurrentdb \
  -p 2114:2113 \
  -e KURRENTDB_CLUSTER_SIZE=1 \
  -e KURRENTDB_RUN_PROJECTIONS=All \
  -e KURRENTDB_START_STANDARD_PROJECTIONS=true \
  -e KURRENTDB_NODE_PORT=2113 \
  -e KURRENTDB_INSECURE=true \
  -e KURRENTDB_ENABLE_ATOM_PUB_OVER_HTTP=true \
  -v kurrentdb-data:/var/lib/kurrentdb \
  -v kurrentdb-logs:/var/log/kurrentdb \
  docker.kurrent.io/kurrent-latest/kurrentdb:latest
```

### Verifying Kurrent is Running

Access the Kurrent UI in your browser:
- URL: `http://localhost:2114`
- No authentication required (insecure mode)

## Running the Tests

### From Visual Studio
1. Ensure Kurrent is running (see above)
2. Open the solution in Visual Studio
3. Build the solution
4. Open Test Explorer (Test > Test Explorer)
5. Run all tests or select specific tests

### From Command Line
```bash
# Ensure Kurrent is running first
docker-compose -f eventstore-stack.yml up -d kurrentdb

# Run the tests
dotnet test DStack.Aggregates.KurrentDB.IntegrationTests/DStack.Aggregates.KurrentDB.IntegrationTests.csproj
```

## Test Coverage

The integration tests cover:

- **Should_Store_And_Load**: Verifies basic store and retrieve operations
- **Store_Should_Reset_List_Of_Changes**: Ensures the changes list is cleared after storing
- **Should_Get_Specified_Version_Of_The_Aggregate**: Tests version-specific retrieval
- **Get_Should_Return_Null_If_Id_Was_Not_Found**: Verifies null return for non-existent aggregates
- **Concurrent_Updates_Should_Throw_ConcurrencyException**: Tests optimistic concurrency control

## Differences from EventStore Tests

The tests are structurally identical to the EventStore integration tests but use:
- `KurrentDB.Client` namespace instead of `EventStore.Client`
- `KurrentDBClient` instead of `EventStoreClient`
- `KurrentDBClientSettings` instead of `EventStoreClientSettings`
- `KurrentAggregateRepository` instead of `ESAggregateRepository`
- Port `2114` instead of `2113` to avoid conflicts

The test behavior and assertions remain the same, ensuring API compatibility between the two implementations.

## Troubleshooting

### Connection Refused
If tests fail with connection errors:
1. Verify Kurrent is running: `docker ps | grep kurrentdb`
2. Check logs: `docker logs kurrentdb` or `docker-compose -f eventstore-stack.yml logs kurrentdb`
3. Verify port 2114 is not in use: `netstat -an | findstr 2114` (Windows) or `lsof -i :2114` (Linux/Mac)

### Port Already in Use
If port 2114 is already in use, you can change it:
1. Update the port mapping in `eventstore-stack.yml` (e.g., `"2115:2113"`)
2. Update the connection string in `appsettings.json` to match the new port
