# Legacy end-to-end suite

This is the Jest/Puppeteer suite restored from commit
`072d113a01b692dc070786764a5f1cc3371fca46`, the master version merged into the
Playwright migration branch. The test cases, interactions, and waits are kept as
the original baseline so they remain an independent comparison during the transition.
The workflow message helper normalizes Windows and Linux directory separators
before checking the original error-text prefix; this is the only test-file adaptation.

Harness adaptations are limited to environment-based application configuration,
headless mode, an explicit Chrome executable, screenshot output paths, and a configurable `origam-utils.dll`
path. Database resets still use the original .NET utility; its configuration must
point at the same test database as the application.

Install with `corepack yarn@1.22.22 install --frozen-lockfile --ignore-scripts`.
Postinstall scripts are skipped because the suite uses an existing Chrome
installation through `PUPPETEER_EXECUTABLE_PATH`, rather than downloading Chromium
or compiling the unused Node database drivers.

See [the test instructions](../Readme.md) for local and CI execution. Preserve this
baseline while comparing the runners; record any necessary compatibility changes
separately from changes to the Playwright suite.
