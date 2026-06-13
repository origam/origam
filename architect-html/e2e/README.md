# End-to-end tests (Playwright)

E2E tests for the Origam HTML Architect frontend.

## Running

```bash
yarn test:e2e          # run all tests headless (auto-starts the dev server)
yarn test:e2e:ui       # interactive UI Mode: pick tests, step through, time-travel
yarn test:e2e:debug    # run with the Playwright Inspector attached
yarn test:e2e:codegen  # record actions into a test by clicking through the app
```

Playwright starts `yarn dev` (HTTPS on https://localhost:5173) automatically and
shuts it down afterwards. Locally it reuses a dev server you already have
running, so `yarn dev` in another terminal speeds up the loop.

> The dev server proxies API calls to `Origam.Architect.Server`. The bundled
> `smoke.spec.ts` only checks the app shell, so it passes without the backend.
> Tests that exercise real flows (model tree, editors) need the backend up.

## Debugging failures with the Trace Viewer

A trace is a full timeline of every action, network request and DOM snapshot.
It's configured (`trace: 'on-first-retry'`) to be captured automatically when a
test fails and gets retried.

```bash
yarn test:e2e:report                       # open the HTML report; click a failed
                                           # test to launch its trace
npx playwright show-trace test-results/<...>/trace.zip   # open a raw trace
```

### In CI

The `Architect HTML E2E` workflow uploads two artifacts on every run:

- **`playwright-report`** — the HTML report with traces embedded. Download it,
  unzip, then `npx playwright show-report path/to/playwright-report`.
- **`playwright-test-results`** — the raw `trace.zip` / video / screenshots, for
  `npx playwright show-trace trace.zip`.

So a failure in CI is debuggable without reproducing it locally.

## Writing stable tests

The smoke test locates elements by visible text. For durable selectors on
dynamic UI (the DnD model tree, editors), prefer adding `data-testid`
attributes to the relevant components and selecting with `page.getByTestId(...)`.
