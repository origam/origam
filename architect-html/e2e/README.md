# End-to-end tests (Playwright)

E2E tests for the Origam HTML Architect frontend. Two suites:

- **`e2e/smoke`** — only touches the app shell, needs **no backend**. Runs in the
  lightweight CI job (`architect-html-e2e.yml`).
- **`e2e/integration`** — drives the frontend against a **real, running
  `Origam.Architect.Server`** (and its database). Runs in the heavy CI job
  (`architect-html-e2e-integration.yml`).

## Running the smoke suite

```bash
yarn test:e2e          # headless (auto-starts the Vite dev server)
yarn test:e2e:ui       # interactive UI Mode: pick tests, step through, time-travel
yarn test:e2e:debug    # run with the Playwright Inspector attached
yarn test:e2e:codegen  # record actions into a test by clicking through the app
```

Playwright starts `yarn dev` (HTTPS on https://localhost:5173) automatically and
shuts it down afterwards. Locally it reuses a dev server you already have
running.

## Running the integration suite (real backend)

```bash
yarn test:e2e:integration       # headless
yarn test:e2e:integration:ui    # UI Mode
```

This uses [`playwright.integration.config.ts`](../playwright.integration.config.ts),
which starts **two** servers before the tests: the Architect Server, then the
Vite dev server that proxies to it.

What it needs:

- A reachable database. Locally that's `SQLEXPRESS` / `origam-demo` as set in the
  server's `OrigamSettings.config`. The first package activation deploys into an
  empty database automatically.
- The built server. By default Playwright launches the local **Debug** build at
  `../backend/Origam.Architect.Server/bin/Debug/net8.0`. If you already run the
  server in Visual Studio, `reuseExistingServer` makes Playwright just wait for
  it instead of starting a second copy.

Override points (env vars):

| Variable | Default | Purpose |
|---|---|---|
| `ARCHITECT_SERVER_URL` | `https://localhost:7099` | Where the server listens (readiness probe). |
| `ARCHITECT_SERVER_DIR` | `../backend/.../bin/Debug/net8.0` | Folder of the server exe to launch. |
| `ARCHITECT_SERVER_CMD` | run the exe in `ARCHITECT_SERVER_DIR` | Full command to start the server. |

> The backend endpoints/ports must match the proxy targets in `vite.config.ts`
> (`https://localhost:7099` and `http://localhost:5003`).

## Debugging failures with the Trace Viewer

A trace is a full timeline of every action, network request and DOM snapshot.
It's captured automatically (`trace: 'on-first-retry'`) when a test fails and
is retried.

```bash
yarn test:e2e:report                       # open the HTML report; click a failed
                                           # test to launch its trace
npx playwright show-trace test-results/<...>/trace.zip   # open a raw trace
```

### In CI

Both workflows upload the HTML report (traces embedded) and the raw
`test-results` (trace.zip / video / screenshots). The integration workflow also
uploads the **Architect Server logs** (`architect-server-*.log`) — check those
first when the failure is backend-side. Download an artifact, then:

```bash
npx playwright show-report path/to/playwright-report
```

## Writing stable tests

Tests locate elements by visible text. For durable selectors on dynamic UI (the
DnD model tree, editors), prefer adding `data-testid` attributes to the relevant
components and selecting with `page.getByTestId(...)`.
