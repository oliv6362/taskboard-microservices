# TaskBoard Software System

Exam Project for the 2025 Software Integration course.

This project is a simple taskboard software system implemented using a microservice architecture.
The system consists of three microservices:

- AssignmentService
- ProjectService
- UserService

The main focus of this project is testing, with emphasis on:

- Consumer-Driven Contract Testing (CDC)
- End-To-End Testing (E2E)

## Consumer-Driven Contract Testing (CDC)
Consumer-Driven Contract Testing focuses on verifying service-to-service communication by testing APIs from the consumer’s perspective.  
The consumer defines the expected behavior of a provider’s API using contracts, and the provider verifies that its implementation matches those expectations.  
This reduces the risk of breaking changes between services and increases confidence when deploying changes.  

### Usage in This Project  
This project uses `PactNet` to implement Consumer-Driven Contract Testing between the microservices.

- **Four contract test projects are included:** two consumer test projects and two provider verification projects.
- **The following service relationships are tested:**
  - AssignmentService (consumer) → ProjectService (provider)
  - ProjectService (consumer) → UserService (provider)
- Consumer contract tests define the expected HTTP interactions and generate Pact files (contracts) using `PactNet`.
- The real consumer client code is executed against a Pact-managed mock provider (HTTP server) to verify that the consumer behaves according to the defined contract.
- Provider verification tests validate the generated contracts against the real provider API implementation.
- Each provider exposes a `ProviderStatesController`, which prepares the database to a known state before verification.


## End-To-End Testing (E2E)
End-to-end testing focuses on validating the behavior of the system as a whole by exercising multiple microservices together through their real HTTP APIs.

While Consumer-Driven Contract Testing verifies that service interactions follow agreed contracts, E2E testing verifies that the complete workflow works correctly when all services, databases, and network communication are running together.


### Usage in This Project
This project uses **xUnit** together with **Testcontainers** to execute end-to-end tests in an isolated Docker-based environment.

The E2E test setup dynamically provisions the full TaskBoard system and includes:

- A dedicated Docker network
- Three SQL Server containers
- Three microservice containers:
  - `UserService`
  - `ProjectService`
  - `AssignmentService`

Each service runs in its own container and is connected through the shared Docker network using internal service names.

The test fixture starts the environment in the correct dependency order:

1. Docker network
2. database containers
3. service containers

Before the tests execute, the fixture waits until each service responds successfully on its `/health` endpoint.  
Once the services are healthy, their mapped base URLs are exposed to the test suite so the system can be exercised through real HTTP requests.

### User Journey Covered
The E2E tests validate a complete user workflow across the microservices:

1. A user is created in `UserService`
2. A project is created in `ProjectService` using the created user as owner
3. An assignment is created in `AssignmentService` using the created project
4. The created assignment is retrieved again and verified against the expected persisted data

This verifies that the services work together correctly across service boundaries and that data flows through the system as expected.
