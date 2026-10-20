# End-to-end tests (Playwright)

End-to-end tests for the **Origam HTML Architect** frontend, powered by [Playwright](https://playwright.dev/).

The integration tests live in [`integration`](./integration) and run against a **real backend**.

This suite is a standalone project. From the `test/architect-html-e2e` directory, before running anything:

```bash
yarn install                      
yarn playwright install chromium 
```

---

## Prerequisites

Before running the integration suite, start both the backend and the frontend:

| Component | What to run |
| --- | --- |
| Backend | `Origam.Architect.Server` |
| Frontend | `architect-html` dev server |

> 📖 Setup instructions live in [`backend/Origam.Architect.Server/HOWTOSTART.md`](../../backend/Origam.Architect.Server/HOWTOSTART.md).

---

## Running the integration suite

From the `test/architect-html-e2e` directory, run:

```bash
yarn test:e2e:integration:ui
```

This opens the Playwright UI runner so you can watch and debug the tests interactively.

## CI execution

The GitHub Actions integration workflow splits the full suite across six parallel
Windows runners using Playwright's `--shard=N/6` option. Each runner has its own
backend, LocalDB database and model checkout. Tests stay sequential within a shard
(`workers: 1`, `fullyParallel: false`) because they reset and modify shared state.
Increasing the worker count against a shared backend would cause tests to interfere.

All shards run to completion even if another shard fails. The final
`architect-html-e2e-integration` check merges their reports into the
`playwright-integration-report` artifact and fails if any shard fails. Test results
and server logs are uploaded separately with the shard number in each artifact name.

The target is approximately 10 minutes rather than the previous 30-minute serial
run. Actual wall time depends on runner availability, setup time, retries and the
slowest shard. Sharding keeps the same coverage and retries, but uses more concurrent
Windows runners and repeats setup on each. Check the first CI run's shard durations
before treating the target as achieved. Playwright assigns whole files to shards
with this configuration, so a particularly slow file can limit the speedup.

---

## Creating a new test

Use Playwright's interactive code generator:

```bash
yarn test:e2e:codegen
```

> 💡 **Tip:** Before recording a new test, rerun your backend and reset (clean) your model so you start from a known, repeatable state.

---

## Troubleshooting

**Codegen can't find the right element?**

Add a stable test id to the element and let the generator target it:

```tsx
<button dataTestId="topbar-deployment-scripts">…</button>
```

With an explicit `dataTestId`, codegen detects and interacts with elements far more reliably.
