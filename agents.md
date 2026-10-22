# Project instructions

## Implementation

- Prefer constructor dependency injection; the long-term goal is to replace service
  locator usage. `Origam.Server` and `Origam.Architect.Server` already use DI:
  register dependencies in their existing service configuration and inject them.
  Use `ServiceManager.Services.GetService` only when working on legacy desktop
  Architect code that requires it, even if nearby server code still uses it.
- Use camelCase without an underscore prefix for new private C# fields.
- Do not use nested ternary conditional operators.

## Formatting and validation

- Format changed C# files with the repository's configured CSharpier version.
  From `backend`, run `dotnet tool restore`, then
  `dotnet csharpier format <changed-file-paths>`. Keep formatting scoped to affected files.
- When adding source files, use the [license-headers skill](.agents/skills/license-headers/SKILL.md).

## Localization

- Localize only text and messages visible to end users. Keep developer-facing text,
  such as diagnostic logs and developer tooling messages, unlocalized.
- Add end-user strings to the project's `Strings.resx` (or equivalent resource file)
  and access them through the generated `Strings.<Key>` properties, optionally using
  `string.Format`. Use `ResourceUtils.GetString` only when explicitly requested.
