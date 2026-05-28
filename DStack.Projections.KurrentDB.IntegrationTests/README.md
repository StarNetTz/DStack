# DStack.Projections.KurrentDB Integration Tests

This project contains integration tests for the KurrentDB projections implementation.

## Prerequisites

To run these tests, you need:

1. **Kurrent (EventStoreDB) instance running locally**
   - The tests expect KurrentDB to be available at `localhost:2114`
   - KurrentDB runs on port 2114 to avoid collision with EventStore on port 2113

2. **Configuration**
   - The connection string is configured in `appsettings.json`
   - Default: `esdb://admin:changeit@localhost:2114?tls=false&tlsVerifyCert=false`
   - Projections Manager URL: `http://localhost:2114`

## Running Kurrent

### Using Docker Compose (Recommended)

```bash
# Start Kurrent
docker-compose -f eventstore-stack.yml up -d kurrentdb

# Verify it's running
docker-compose -f eventstore-stack.yml ps
```

### Verify Kurrent is Running

Access the Kurrent UI in your browser:
- URL: `http://localhost:2114`
- Username: `admin`
- Password: `changeit`

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
dotnet test DStack.Projections.KurrentDB.IntegrationTests/DStack.Projections.KurrentDB.IntegrationTests.csproj
```

## Test Coverage

The integration tests cover:

- **InitializationTest**: Writes test events to the Kurrent database
- **KurrentSubscriptionTests**: Tests subscription to event streams
- **KurrentProjectionBuilderTests**: Tests projection definition building

## Architecture

### Key Components

1. **KurrentSubscription**: Handles real-time event subscriptions from streams
2. **KurrentSubscriptionFactory**: Creates subscription instances
3. **KurrentProjectionBuilder**: Builds projection definitions for stream transformations
4. **JSProjectionsFactory**: Manages JavaScript-based continuous projections

### Differences from EventStore Tests

The tests are structurally identical to the EventStore integration tests but use:
- `KurrentDB.Client` namespace instead of `EventStore.Client`
- `KurrentDBClient` instead of `EventStoreClient`
- `KurrentDBClientSettings` instead of `EventStoreClientSettings`
- `StreamState` instead of `StreamRevision`
- Port `2114` instead of `2113` to avoid conflicts

## Troubleshooting

### Connection Refused
If tests fail with connection errors:
1. Verify Kurrent is running: `docker ps | grep kurrentdb`
2. Check logs: `docker-compose -f eventstore-stack.yml logs kurrentdb`
3. Verify port 2114 is accessible: `curl http://localhost:2114`

### Projection Not Created
1. Check Kurrent UI projections page: `http://localhost:2114/web/index.html#/projections`
2. Ensure projections are enabled in Kurrent configuration
3. Check test logs for errors

### Authentication Errors
- Default credentials: `admin` / `changeit`
- Ensure connection string includes credentials
- Verify Kurrent is running in insecure mode for testing
