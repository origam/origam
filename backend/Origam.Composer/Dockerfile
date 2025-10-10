# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file and all project files
COPY ../Origam.sln .
COPY ../Directory.Build.props .
COPY ../global.json .

## Copy all project directories
COPY ../Origam.Composer/ Origam.Composer/
COPY ../Origam.OrigamEngine/ Origam.OrigamEngine/
COPY ../Origam.ReflectorCache/ Origam.ReflectorCache/
COPY ../Origam.Security.Common/ Origam.Security.Common/
COPY ../Origam.Security/ Origam.Security/
COPY ../Origam.Workbench.Services/ Origam.Workbench.Services/
COPY ../Origam.Common/ Origam.Common/
COPY ../Origam.UI.Common/ Origam.UI.Common/
COPY ../Origam.DA.Common/ Origam.DA.Common/
COPY ../Origam.DA.Service/ Origam.DA.Service/
COPY ../Origam.Schema/ Origam.Schema/
COPY ../Origam.Schema.EntityModel/ Origam.Schema.EntityModel/
COPY ../Origam.Schema.WorkflowModel/ Origam.Schema.WorkflowModel/
COPY ../Origam.Schema.GuiModel/ Origam.Schema.GuiModel/
COPY ../Origam.Schema.MenuModel/ Origam.Schema.MenuModel/
COPY ../Origam.Schema.RuleModel/ Origam.Schema.RuleModel/
COPY ../Origam.Schema.DeploymentModel/ Origam.Schema.DeploymentModel/
COPY ../Origam.Schema.LookupModel/ Origam.Schema.LookupModel/
COPY ../Origam.Services/ Origam.Services/
COPY ../EnterpriseLibrary/ EnterpriseLibrary/
COPY ../ScheduleTimer/ ScheduleTimer/
#COPY ../Origam.Server.Common/ Origam.Server.Common/
#COPY ../Origam.Rule/ Origam.Rule/
#COPY ../Origam.Workflow/ Origam.Workflow/

# Restore dependencies
RUN dotnet restore Origam.Composer/Origam.Composer.csproj

# Build and publish
RUN dotnet publish Origam.Composer/Origam.Composer.csproj -c "Release Server" -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

# Install git (needed for git operations in the app)
RUN apt-get update && \
    apt-get install -y git && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Set the entrypoint
ENTRYPOINT ["dotnet", "Origam.Composer.dll"]

# Note: Database is expected to run on the host machine, accessible via host.docker.internal
CMD ["create", "--help"]
