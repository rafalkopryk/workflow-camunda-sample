# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A .NET 10 / Aspire 13 sample showing the same credit-application business flow implemented with three interchangeable workflow engines (Camunda 8, a custom saga, Temporal), two interchangeable frontends (Next.js or Blazor), and three interchangeable database providers (Postgres, SQL Server, MongoDB). Inter-service communication runs over Kafka or Azure Service Bus. Aspire orchestrates everything locally.

The whole point of the codebase is the *swappability*: changing a single parameter in `Modules/Aspire/Aspire/Aspire.AppHost/appsettings.json` reroutes the AppHost to wire up an entirely different process engine or frontend. Keep that in mind when making changes — the abstractions exist to keep the three workflow paths comparable.

## Run

Prerequisites: Docker Desktop running.

```bash
# Primary entry point — Aspire AppHost orchestrates every service + container
dotnet run --project Modules/Aspire/Aspire/Aspire.AppHost

# Build the whole solution
dotnet build Credit.slnx
```

The solution file is `Credit.slnx` (XML format), not `.sln`. Central package management is on (`Directory.Packages.props`); add new package versions there, not in individual `.csproj` files.

### Switching providers

Edit `Modules/Aspire/Aspire/Aspire.AppHost/appsettings.json` `Parameters`:

| Parameter | Values | Effect |
|---|---|---|
| `processProvider` | `camunda` (default), `saga`, `temporal` | Which `Processes.*.WebApi` Aspire wires up |
| `creditFrontProvider` | `react`, `blazor` | Next.js app vs Blazor server |
| `databaseProvider` | `postgres` (default), `sqlserver`, `mongodb` | Database container + EF provider |
| `kafkaUiEnabled` | bool | Adds Kafka UI container |

The `AddProcess`, `AddCreditFront`, and `AddDatabaseServer` extension methods in `Modules/Aspire/Aspire/Aspire.AppHost/Program.cs` switch on these parameters.

`ServiceBusProvider` (kafka vs Azure Service Bus) is set per-service via `WithKafkaReference` in `ResourceBuilderExtensions.cs`; Wolverine config in each module's `ServiceCollectionExtensions.ConfigureWolverine` branches on `configuration.IsKafka()`.

## Architecture

### Module layout

Each business module follows a `*.WebApi` (host) + `*.Application` (logic) split. The Application project owns DI registration via `ServiceCollectionExtensions.AddApplication` / `AddInfrastructure` and Wolverine wiring via `ConfigureWolverine`. The WebApi project's `Program.cs` is thin — it calls `AddServiceDefaults()`, those two extension methods, and maps endpoints.

- `Modules/Applications/` — credit applications domain (register, sign, cancel, decision)
- `Modules/Calculations/` — simulation calculations
- `Modules/Processes/` — orchestrates the credit flow; **three parallel implementations** (`Processes.WebApi` = Camunda, `Processes.Saga.WebApi` = stateless saga, `Processes.Temporal.WebApi` = Temporal). All three subscribe to the same Kafka topics and publish the same events — they are interchangeable from the rest of the system's view.
- `Modules/Front/` — `credit.front.next` (Next.js 15 + React 19 + MUI) and `Credit.Front.Blazor` (Blazor WASM)
- `Modules/Aspire/Aspire/Aspire.AppHost` — orchestration entry point
- `Modules/Aspire/Aspire/Aspire.ServiceDefaults` — shared OpenTelemetry + service discovery + resilience
- `Common/Common.Application` — custom CQRS mediator, configuration helpers, shared building blocks
- `external/camunda-startup/` — vendored Camunda integration libraries (see below)

### Custom CQRS mediator

This project does **not** use MediatR. It has its own `Mediator` in `Common/Common.Application/Cqrs/IRequestHandler.cs` that resolves `IRequestHandler<TInput, TOutput>` from a freshly-created scope. Register handlers with `services.RegisterHandlersFromAssemblies(typeof(...).Assembly)`. Send via `mediator.Send<MyCommand, MyResponse>(cmd)`. Responses are typically discriminated unions expressed as nested records (e.g. `RegisterApplicationCommandResponse.OK` / `.ResourceExists`).

### Messaging (Wolverine)

Cross-module communication goes through Wolverine over Kafka or Azure Service Bus. Each module's `ConfigureWolverine` declares its publishes and listens. Topics like `applications`, `decisions`, `simulations`, `contracts`, `customer-verifications` are the integration surface — touching them affects multiple modules.

### Camunda integration

Camunda code lives in `external/camunda-startup/`, which is a `git subtree` from <https://github.com/rafalkopryk/CamundaStartup>. Update it with:

```bash
git subtree pull --prefix external/camunda-startup camunda-startup master --squash
```

Do **not** edit files under `external/camunda-startup/` directly unless you intend to push them upstream. The subtree provides:

- `CamundaClient.Extensions` — `IJobHandler`, DI scoping, OTel `ActivitySource`
- `CamundaStartup.Aspire.Hosting.Camunda` — `AddCamunda()`, `WithRdmbsDatabase()`, secondary-storage backends

Job handlers in `Modules/Processes/Processes.Application/UseCases/CreditApplications/**/` are decorated with `[JobWorker(Type = "...:1")]` and registered in `Processes.Application.Extensions.ServiceCollectionExtensions.AddApplication` via `services.AddCamunda(...).AddWorker<TJobHandler>(...)`. They typically translate a Camunda job into a Wolverine `PublishAsync` and let downstream modules do the actual work.

The BPMN file `Modules/Processes/Processes.WebApi/BPMN/credit-application.bpmn` is deployed at startup by `DeployBPMNDefinitionService` (driven by `ProcessDefinitionsOptions` / `PathDefinitionsOptions` in `appsettings.json`).

### Camunda-related skills

The `external/camunda-startup/.claude/skills/` directory ships three skills that are exposed automatically:

- `create-job-handler` — generates `IJobHandler` scaffolding for a BPMN service task
- `bpmn-review` — reviews BPMN files against Camunda 8 best practices
- `aspire` — wraps the `aspire` CLI for distributed-app orchestration

Prefer these for Camunda-specific work over hand-rolling.

## Conventions

- Markdown / docs in English (carried over from the camunda-startup subtree).
- Prefer Aspire NuGet integrations over hand-rolled container wiring.
- New package versions go in `Directory.Packages.props` (central package management is enabled).
- When adding a new workflow step, expect to touch *all three* `Processes.*.WebApi` implementations, or explicitly note in the PR which providers are out of scope.