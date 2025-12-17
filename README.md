# TaskBoard Software System
Exam Project for the 2025 Software Integration course.  

This project is a simple taskboard software system implemented using a microservice architecture.

### The system consists of three microservices services:
- AssignmentService
- ProjectService
- UserService

### The main focus of this project is testing, with emphasis on:
- Consumer-Driven Contract Testing (CDC)
- End-To-End Testing (E2E)


## Consumer-Driven Contract Testing (CDC)
Consumer-Driven Contract Testing focuses on verifying service-to-service communication by testing APIs from the consumer’s perspective.  
The consumer defines the expected behavior of a provider’s API using contracts, and the provider verifies that its implementation matches those expectations.  
This reduces the risk of breaking changes between services and increases confidence when deploying changes.  

### Usage in This Project  
This project uses PactNet to implement Consumer-Driven Contract Testing between the microservices.
- **Four contract test projects are included:** two consumer test projects and two provider verification projects.
- **The following service relationships are tested:**
  - AssignmentService (consumer) → ProjectService (provider)
  - ProjectService (consumer) → UserService (provider)
- Consumer contract tests define the expected HTTP interactions and generate Pact files (contracts) using PactNet.
- The real consumer client code is executed against a Pact-managed mock provider (HTTP server) to verify that the consumer behaves according to the defined contract.
- Provider verification tests validate the generated contracts against the real provider API implementation.
- Each provider exposes a ProviderStatesController, which prepares the database to a known state before verification.


## End-To-End Testing (E2E)
