# agents.md

## 1. Introduction

Origam is an enterprise software prototyping and development platform that lets teams model data, auto‐generate back-ends, and rapidly build custom front-end views (both web and mobile). Codex will be used to:

- Read and navigate the monorepo structure to locate relevant code.
- Generate, refactor, and document C# backend services following Origam’s conventions.
- Work with the frontend (TypeScript/JavaScript + HTML) to build UI components consistent with existing patterns.
- Understand how plugins are structured so any new extension follows the same organization.
- Respect testing conventions under `model-tests` and `test` directories.

> **Note:** Origam’s codebase is split into multiple logical layers (backend, frontend, plugins, model, tests). Codex should choose the appropriate directory based on the user’s request.

## 2. Repository Structure & Key Directories

Below is the high-level tree (simplified) of the Origam monorepo citeturn3search0:

```
.
├── .github/                     # GitHub workflows, CI/CD configs
├── backend/
│   ├── Origam.OrigamEngine/     # Core engine (C#)
│   ├── Origam.DA.Service/       # Data access layer (C#)
│   ├── Origam.BI.Excel/         # Excel export service (C#)
│   ├── MetaModelUpgrade/        # Scripts for upgrading metadata (C#)
│   └── …                        # Other backend subprojects
├── frontend-html/               # All HTML/TypeScript/React frontend code
│   ├── src/                     # React components, pages, services
│   └── package.json             # Frontend build & dependency definitions
├── model-root/                  # Domain models, metadata definitions
├── model-tests/                 # Automated tests for business models
├── plugins/                     # Plugin projects: structure for third-party extensions
├── build/                       # Build scripts, CI/CD utilities
├── docker/                      # Docker configurations for server images
├── test/                        # End-to-end and integration tests
├── CONTRIBUTING.md              # Contribution guidelines
├── HOWTOSTART.md                # Setup instructions and how to run the platform
├── LICENSE                      # Apache-2.0
└── README.md                    # Project overview citeturn3search0
```

- **backend/**:  
  - All C# code lives here. Each subfolder (e.g., `Origam.OrigamEngine`) represents a .NET project.  
  - Namespace convention: `Origam.[ProjectName]`.  
  - Use dependency injection via the built-in .NET Core service container.  
  - Focus backend logic around services, controllers, repositories, and model upgrades.

- **frontend-html/**:  
  - Contains a client-side single-page application built with TypeScript and React (Vite).  
  - Follow component naming: PascalCase for components, camelCase for hooks and utility functions.  
  - CSS modules or styled components are not used—styles typically reside in `.scss` files.  

- **model-root/** and **model-tests/**:  
  - Defines the business metadata (entities, attributes) using Origam’s metadata format.  
  - Tests verify that model changes propagate correctly to code generation and runtime.  

- **plugins/**:  
  - Each plugin has its own folder with the following structure:  
    ```
    plugins/
      └── <PluginName>/
          ├── src/            # Source code (usually TypeScript or C#)
          ├── package.json    # Plugin dependencies
          └── README.md       # Plugin description and usage
    ```

- **build/**:  
  - Contains scripts for building, packaging, and releasing Origam components.  
  - Codex should not alter these unless explicitly asked; follow existing shell/PowerShell conventions.

- **docker/**:  
  - Holds Dockerfiles and compose files to spin up Origam’s server images.  
  - Tagging convention: `origam/server:<version>.<build>.<platform>`.

- **test/**:  
  - End-to-end test suites, often using NUnit/XUnit (for backend) or Jest/Playwright (for frontend).  
  - Tests reference models in `model-root/` and generated code in `backend/` before running.

## 3. Technologies & Frameworks

### 3.1 Backend (C# / .NET)

- **.NET Version**: 6.0 (LTS).  
- **Project style**: SDK-style `.csproj` files.  
- **Dependency Injection**: Use `IServiceCollection` in `Startup.cs` or `Program.cs`.  
- **ORM/Data Access**: Origam uses its own data access abstraction in `Origam.DA.Service`—avoid introducing Entity Framework unless requested.  
- **Logging**: Use `Microsoft.Extensions.Logging.ILogger<T>`.  
- **Asynchronous Code**: Prefer `async`/`await` for service and repository operations.  
- **Exception Handling**: Wrap top-level controllers with try/catch and return structured error responses.  
- **Unit Testing**: Use XUnit with the `[Fact]` attribute. Tests live under `model-tests/` or `test/backend/`.

### 3.2 Frontend (TypeScript / React)

- **Package Manager**: Yarn (refer to `frontend-html/package.json`).  
- **Build**: Vite (see `vite.config.ts`).  
- **Component Style**: Function components + React Hooks.  
- **Naming Conventions**:  
  - Components: `PascalCase` (e.g., `UserCard.tsx`).  
  - Hooks: `useCamelCase` (e.g., `useFetchData.ts`).  
  - Utility files: `camelCase` (e.g., `apiClient.ts`).  
- **Styling**: SCSS (one file per component or shared folder). Avoid inline styles.  
- **State Management**: Minimal—prefer local state via `useState` or context if needed.  
- **Testing**: Jest + React Testing Library (under `frontend-html/__tests__/`).

### 3.3 Plugins

- Can be either C#‐based or TypeScript folders. Follow the same conventions as backend or frontend, respectively.  
- Each plugin must export a manifest (e.g., JSON or `plugin.json`) that Origam uses to register it at runtime.

## 4. Coding Conventions & Style Guidelines

When Codex generates or modifies code, it should adhere to these patterns:

### 4.1 C# Conventions

- **File / Class Matching**: One public class per file, named exactly as the file.  
- **Namespaces**: Mirror folder hierarchy. E.g., `backend/Origam.OrigamEngine/Services` ⇒ `namespace Origam.OrigamEngine.Services`.  
- **Brace Style**: Allman style (opening brace on new line).  
- **Access Modifiers**: Explicit: always state `public`, `private`, etc.  
- **Async Methods**: Suffix with `Async` (e.g., `GetUserAsync`). Return `Task` or `Task<T>`.  
- **Properties vs. Fields**: Use auto-properties (`public int Id { get; set; }`) except for private readonly fields (prefix with `_`). E.g., `private readonly ILogger<MyService> _logger;`  
- **Dependency Injection**: Constructor injection only. Avoid the service locator pattern.

### 4.2 TypeScript/React Conventions

- **Type Annotations**: Use interfaces or type aliases for props (`interface UserCardProps { name: string; }`).  
- **Imports**: Use absolute aliases defined in `tsconfig.json` (e.g., `import { Button } from '@/components/Button';`).  
- **JSX**: Always use self-closing tags when no children (`<Spinner />`).  
- **Hooks**: Call hooks at the top level of function components.  
- **Error Handling**: Wrap `fetch` calls or API requests in try/catch and surface user-friendly messages.  
- **CSS Classnames**: BEM style is optional; maintain consistency with existing code (mostly kebab-case).  
- **Testing**: Each React component should have a corresponding test file named `<Component>.test.tsx` under `frontend-html/__tests__/`.

## 5. Common Tasks & How Codex Should Approach Them

Below are examples of typical tasks Codex might be asked to perform, along with guidance on how to proceed:

1. **“Generate a new service in the backend to handle X feature.”**  
   - Create a folder `backend/Origam.OrigamEngine/Services/<FeatureName>Service.cs`.  
   - Define an interface `I<FeatureName>Service` under `backend/Origam.OrigamEngine/Services/Interfaces`.  
   - Implement `public class <FeatureName>Service : I<FeatureName>Service` with required methods.  
   - Register the new service in `Startup.cs` (e.g., `services.AddScoped<I<FeatureName>Service, <FeatureName>Service>();`).  
   - Add unit tests under `model-tests/Origam.OrigamEngine/Services` following XUnit conventions.

2. **“Refactor existing C# method to use async/await.”**  
   - Ensure the return type changes from `void` or `T` to `Task` or `Task<T>`.  
   - Add `async` modifier and refactor synchronous calls to their asynchronous counterparts (e.g., `await repository.GetAsync(id)`).  
   - Update any calling code to `await` the method.  
   - Update unit tests to `async Task` and use `await` in assertions.

3. **“Create a new React component for a UserCard.”**  
   - Under `frontend-html/src/components/`, create `UserCard.tsx` with a function component and typed props.  
   - Add a SCSS file `UserCard.scss` in the same folder; import it into the component (`import './UserCard.scss';`).  
   - If a service call is needed, create or update `frontend-html/src/services/apiClient.ts` to include a new `getUser` function (using `fetch` or `axios` as per existing code).  
   - Generate a corresponding test `UserCard.test.tsx` under `frontend-html/__tests__/` that uses React Testing Library.

4. **“Add a plugin to extend Origam’s behavior.”**  
   - Under `plugins/`, create a new folder `MyPlugin/`.  
   - Within `MyPlugin/`, add `src/PluginMain.cs` (if C#) or `index.ts` (if TypeScript), exporting the plugin’s main entry point.  
   - Provide a `plugin.json` manifest that follows Origam’s plugin schema.  
   - Update any registry file (if one exists) under `backend/Origam.OrigamEngine/Plugins` so the plugin is discovered at runtime.

5. **“Improve test coverage for the metadata model.”**  
   - Look under `model-tests/`; identify missing test classes next to each `model-root/` entity.  
   - Create new XUnit `[Fact]` or `[Theory]` tests to validate model constraints, default values, and code generation outputs.  
   - Ensure that all `model-root` classes produce valid business objects.  
   - Run `dotnet test` to confirm everything passes.

## 6. How to Load & Use agents.md with Codex CLI

1. **Location**: Place this file at the repository root and name it exactly `agents.md`.  
2. **Priority**: Codex merges instructions in the following order citeturn4search3:  
   1. `~/.codex/instructions.md` (user’s global instructions)  
   2. `agents.md` at the repository root (this file)  
   3. `agents.md` in the current working directory (for sub-package specifics)

3. **Behavior**:  
   - When running `codex` inside the repository root, Codex will automatically load `agents.md` to understand Origam’s domain, folder layout, and coding guidelines.  
   - If asked to work in `plugins/MyPlugin/`, Codex can merge this root file with a local `agents.md` (if created) to provide more granular plugin guidance.

## 7. Additional Tips for Codex

- **Read Existing Code First**: When modifying or extending anything, Codex should scan relevant files (e.g., existing services in `Origam.OrigamEngine/Services`) to match naming patterns and method signatures.  
- **Minimize Disruptive Changes**: Avoid mass renaming or removing code unless the user explicitly requests refactoring; incremental additions are preferred.  
- **Preserve Comments & Regions**: Many C# files use `#region ... #endregion` blocks; keep them intact.  
- **Versioning**: Whenever creating or updating Docker tags in `docker/`, follow the `YYYY.MAJOR.MINOR` scheme that Origam uses.  
- **Documentation**: If generating XML comments for methods (using `/// <summary> ... </summary>`), keep them concise and relevant to business logic.  
- **Error Messages**: Origam often surfaces localized error strings; when adding new exceptions, follow the existing pattern (e.g., `throw new InvalidOperationException("Feature X is not available.")`).
- **License Headers**: New source files must include the standard ORIGAM license header with the year range updated to the current year (e.g., `2005 - 2025`).

---

*End of agents.md*
