# Origam Backend

This section of the project consists of server-side libraries, server implementations, and the `origam-utils` command-line tool.

Linting is enforced by automated tests during the PR process. When compiling the code locally in debug mode, linting is applied automatically, so any pushed code should pass the automated tests without issues.

The automated tests use their own model, `model-tests`, which is separate from `model-root`. If new code depends on changes in the model, those changes must also be added to `model-tests`; otherwise, the automated tests will fail.

## Building with .NET 10

Run these commands from `backend`:

```powershell
dotnet tool restore
dotnet build Origam.Architect.Server/Origam.Architect.Server.csproj -c "Debug Architect Server"
dotnet build Origam.Architect.Server/Origam.Architect.Server.csproj -c "Release Architect Server"
dotnet build Origam.Server/Origam.Server.csproj -c "Debug Server"
dotnet build Origam.Server/Origam.Server.csproj -c "Release Server"
dotnet build origam-utils/origam-utils.csproj -c "Debug Server"
dotnet build origam-utils/origam-utils.csproj -c "Release Server"
dotnet csharpier check .
```

Use an Architect Server configuration for shared model tests that edit model items,
and a Server configuration for `Origam.ServerTests` and `Origam.WorkflowTests`.
Workflow integration tests require a deployed test database and `OrigamSettings.config`.
AI integration tests require the configuration described in [their README](Origam.AI.Agent.Tests/README.md).
