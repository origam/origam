# Dependabot updates

[.github/dependabot.yml](dependabot.yml) schedules routine NuGet and npm updates weekly, with a seven-day release cooldown. The groups below batch related updates into fewer PRs. Updates outside these groups can still get individual PRs, subject to the configured ignore rules.

| Directory | Group | Packages | Updates |
| --- | --- | --- | --- |
| `backend` | `csharpier` | `csharpier`, `CSharpier.MsBuild` | Patch, minor, major |
| `backend` | `nuget-test-patches`, `nuget-test-minors` | Microsoft.NET.Test.Sdk, Moq, NUnit, NUnit3TestAdapter | Separate patch and minor batches |
| `backend` | `log4net` | log4net | Patch and minor |
| `frontend-html`, `architect-html`, `chat-html` | `frontend-react`, `architect-react`, `chat-react` | `react`, `react-dom`, `@types/react`, `@types/react-dom` | Patch and minor per app |
| `frontend-html` | `frontend-test-patches`, `frontend-test-minors` | `@testing-library/dom`, `@testing-library/jest-dom`, `@testing-library/react`, `@testing-library/user-event`, `@types/jest`, `identity-obj-proxy`, `jest`, `jest-environment-jsdom`, `ts-jest` | Separate patch and minor batches |
| `frontend-html`, `architect-html` | `frontend-vite`, `architect-vite` | `vite`, `@vitejs/plugin-react` | Patch and minor per app |
| `frontend-html`, `architect-html`, `chat-html` | `frontend-dev-updates`, `architect-dev-updates`, `chat-dev-updates` | Remaining npm development dependencies | Patch and minor per app |

## Eligibility label

[.github/workflows/dependabot-label.yml](workflows/dependabot-label.yml) applies `eligible-for-auto-merge` to qualifying Dependabot PRs. The group names show where these updates usually appear. The workflow checks the metadata for each dependency, so eligible individual PRs can also receive the label.

| Group | Eligible updates |
| --- | --- |
| `nuget-test-patches` | Stable patches for the four NuGet test packages |
| `frontend-test-patches` | Stable patches for the nine frontend test development dependencies |
| `frontend-vite`, `architect-vite` | Stable patches and minors for Vite and its React plugin as development dependencies |

Every dependency in the PR must qualify. The rule requires a verified, single-commit Dependabot PR targeting `master` that is open and ready for review. Major upgrades, prereleases, `0.x` versions, and updates with maintainer changes are ineligible. The workflow rechecks eligibility when a PR changes and removes the label if it no longer qualifies.

**Approval and merging are manual.** The label reflects dependency eligibility and can appear while CI is still running.
