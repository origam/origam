# Origam AI — configuration

`Origam.AI.Agent` is a class library hosted by `Origam.Architect.Server`. It has no
configuration of its own: everything is read from the Architect server's settings.

## Where the settings live

Both files are gitignored. Create them by copying the tracked templates:

| copy from | to |
|---|---|
| `Origam.Architect.Server/ConfigTemplates/_appsettings.json` | `Origam.Architect.Server/appsettings.json` |
| `Origam.Architect.Server/ConfigTemplates/_appsettings.Development.json` | `Origam.Architect.Server/appsettings.Development.json` |

## Settings

```json
{
  "Ai": {
    "Endpoint": "https://api.openai.com/v1",
    "Model": "gpt-5.6-luna",
    "Community": { "BaseUrl": "https://community.origam.com" }
  }
}
```

| key | meaning |
|---|---|
| `Ai:Endpoint` | OpenAI-compatible API base url |
| `Ai:Model` | model id sent with every request |
| `Ai:ApiKey` | see below |
| `Ai:Community:BaseUrl` | Origam community site searched by the community tool |

## The API key

Put it in `appsettings.Development.json`, never in a `ConfigTemplates/` file:

```json
{
  "Ai": {
    "ApiKey": "sk-..."
  }
}
```
