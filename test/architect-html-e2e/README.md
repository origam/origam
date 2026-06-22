# End-to-end tests (Playwright)

End-to-end tests for the **Origam HTML Architect** frontend, powered by [Playwright](https://playwright.dev/).

The integration tests live in [`integration`](./integration) and run against a **real backend**.

This suite is a standalone project: install its dependencies with `yarn install`
from this directory before running anything. The frontend dev server it launches
still comes from `architect-html`, so that project must have its dependencies
installed too.

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
