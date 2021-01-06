Run latest EventStore 2020 in docker.

use:
docker run --name esdb-node -it -p 2113:2113 -p 1113:1113 eventstore/eventstore:latest --insecure --run-projections=All --enable-atom-pub-over-http

Install latest Grpc.Net.Client or gRPC connection on unsecured db instance will fail with an error HTTP1.1 not supported