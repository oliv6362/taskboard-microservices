# TaskBoard Microservices

Exam project for the 2025 **Software Integration** course.

This repository contains a **TaskBoard software system** implemented as a **microservice-based .NET solution**.  
The project focuses on how testing strategies can be applied in a distributed system, with particular emphasis on:

- **Consumer-Driven Contract Testing (CDC)**
- **End-to-End Testing (E2E)**

The purpose of the project is to demonstrate how these two test approaches can be combined to validate both **service-to-service communication in isolation** and **complete workflows across multiple microservices**.

---

## System Overview

The system consists of three microservices:

- `UserService`
- `ProjectService`
- `AssignmentService`

Each service is responsible for its own bounded area of the domain and communicates with other services over HTTP.

A typical workflow in the system is:

1. A user is created in `UserService`
2. A project is created in `ProjectService` with that user as owner
3. An assignment is created in `AssignmentService` for that project
4. The assignment is retrieved again and validated

This workflow forms the basis of the project’s end-to-end testing strategy.

---

## Testing Focus

This project is centered around testing in a microservice architecture, where different test types provide different forms of confidence.

### Consumer-Driven Contract Testing (CDC)

CDC is used to verify **service interactions in isolation**.

In this approach, the **consumer** defines its expectations of a provider’s API in the form of a contract, and the **provider** verifies that its implementation satisfies that contract. This helps reduce the risk of breaking changes between independently developed services.

#### CDC relationships covered in this project

- `AssignmentService` (consumer) → `ProjectService` (provider)
- `ProjectService` (consumer) → `UserService` (provider)

#### How CDC is implemented

This project uses **PactNet** for contract testing.

For each service interaction, there are separate test projects for:

- **consumer contract tests**
- **provider verification tests**

The consumer tests:

- define expected HTTP interactions
- execute the real consumer client code against a Pact mock server
- generate Pact files that document the agreed contract

The provider verification tests:

- load the generated Pact files
- run verification against the real provider API
- prepare provider state before verification through a `ProviderStatesController`

This allows service contracts to be validated without requiring the full system to be started.

---

## End-to-End Testing (E2E)

E2E testing is used to verify the **system as a whole**.

Where CDC focuses on isolated service interactions, E2E testing validates that the full workflow works correctly when all services, databases, and network communication are running together.

This project uses **xUnit** together with **Testcontainers** to provision an isolated Docker-based test environment.

### E2E environment

The E2E test setup provisions:

- a dedicated Docker network
- three SQL Server containers
- three microservice containers:
  - `UserService`
  - `ProjectService`
  - `AssignmentService`

The environment is started in dependency order so that databases are available before the services start.  
Before the test runs, the fixture waits until each service responds successfully on its `/health` endpoint.

### E2E workflow covered

The end-to-end test validates a complete user journey across the system:

1. Create a user
2. Create a project for that user
3. Create an assignment for that project
4. Retrieve the assignment and verify the persisted data

This gives stronger confidence that the services work correctly together across service boundaries.

---

## Trade-offs Discussed in the Project

A central part of the project is not only implementing tests, but also evaluating the trade-offs of the chosen strategy.

### E2E trade-offs

End-to-end tests provide high realism and strong confidence because they run against real services, databases, and network communication.  
However, this comes at a cost:

- slower feedback
- more infrastructure setup
- greater fragility
- weaker scalability as the number of services grows

In other words, E2E tests are valuable for validating important workflows, but they are less suitable as the primary feedback mechanism in larger microservice systems.

### CDC trade-offs

CDC tests provide faster feedback and make it easier to detect integration issues between services early.  
They also support a higher degree of independent development and deployment, because services can be verified against contracts without running the full environment.

However, CDC does **not** prove that the complete system works end-to-end.  
For that reason, CDC and E2E should be seen as complementary rather than competing approaches.

---

## Technologies Used

- .NET
- xUnit
- Testcontainers
- PactNet
- Docker
- SQL Server

---

## Conclusion

The project shows that an effective testing strategy for microservices should not rely on a single test type.

Instead, the repository demonstrates a balanced approach where:

- **E2E tests** are used selectively to validate important system workflows
- **CDC tests** are used to validate service interactions in isolation

Together, these approaches provide a better balance between:

- confidence
- feedback speed
- isolation
- deployability

---

## Getting Started

### Prerequisites

- .NET SDK
- Docker
- Docker Compose

### Environment configuration

Before running the application or the test suite, create the required `.env` files with the following values.

#### Root `.env`

Create a `.env` file in the repository root:

```env
DB_HOST=host.docker.internal
DB_PORT=1433
DB_USER=sa
DB_PASSWORD=your-chosen-password
DB_PLATFORM=linux/amd64
DB_IMAGE=mcr.microsoft.com/mssql/server:2022-latest
```

#### `tests/ProjectService.ProviderContractTests/.env`

```env
TEST_SQL_PASSWORD=your-chosen-password
```

#### `tests/Taskboard.E2ETests/.env`

```env
SA_USERNAME=sa
SA_PASSWORD=your-chosen-password
```

#### `tests/UserService.ProviderContractTests/.env`
```env
TEST_SQL_PASSWORD=your-chosen-password
```

### Run the system
`docker compose up --build`

### Run the tests
`dotnet test`
