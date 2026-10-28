# Running the end-to-end tests locally

The browser tests use Playwright and reset their test data directly through Node database drivers.
The original Jest/Puppeteer suite is retained separately in `tests_e2e_legacy` for comparison.

1. Start Origam with the automatic-test model deployed.
2. In `test/tests_e2e`, run `yarn install`.
3. Copy `.env.mssql.example` to `.env.mssql` and/or `.env.postgresql.example` to `.env.postgresql`.
4. Edit each copied file: set `OrigamSettings__DataConnectionString` to the application's test database
   connection string and adjust `ORIGAM_E2E_URL` and the login credentials for that application.
5. Start the UI for the database you want to test:

   | Database | Yarn command | Windows launch script |
   | --- | --- | --- |
   | Microsoft SQL Server | `yarn test:ui:mssql` | `.\start-ui-mssql.cmd` |
   | PostgreSQL | `yarn test:ui:postgresql` | `.\start-ui-postgresql.cmd` |

The launchers load their matching `.env` file, override inherited variables with its values, and
select the database driver. They run Playwright locally even if `CI` is set in your shell.
Additional Playwright arguments are forwarded, for example `yarn test:ui:mssql 01_basicCrud.test.js`.
The Windows scripts can also be launched from another directory.

SQL Server's Windows authentication example requires ODBC Driver 18 for SQL Server and the optional
`msnodesqlv8` dependency. You can also use SQL authentication as shown in the template.

For the standard `yarn test:e2e`, `yarn test:debug`, or `yarn test:ui` commands, copy either template
to `.env` instead. Playwright automatically loads `.env` from `test/tests_e2e`; existing environment
variables take precedence for these standard commands, including in CI.
The local `.env`, `.env.mssql`, and `.env.postgresql` files are ignored by Git.

Set `ORIGAM_E2E_HEADLESS=true` in the active `.env` file to keep the test browser hidden while
using Playwright UI. Set it to `false` to show the test browser. Restart Playwright after changing
the setting. Both profile templates default to `true`; when the variable is absent, the existing
behavior remains: headless when `CI=true`, visible otherwise. `--headed` or `--debug` can override
headless mode when you want to inspect the live browser.

Useful commands:

- `yarn test:debug` opens Playwright Inspector.
- `yarn test:ui` opens Playwright UI mode.
- `yarn playwright test 01_basicCrud.test.js` runs one file.
- `yarn playwright test -g "Should perform basic CRUD"` runs one named test.
- `yarn show-report` opens the HTML report from the latest run.

Failures retain a screenshot, video, and trace under `test-results`. The HTML report is written to
`playwright-report`, and CI also publishes both directories as artifacts.

# Running the end-to-end tests in Docker

1. Copy `model-tests` to `test/model`.
2. Create `test/server_bin`.
3. Build the server and copy its output to `test/server_bin`.
4. Build the frontend and copy `frontend-html/dist` to `test/server_bin/clients/origam`.
5. From `test`, run the relevant compose file, for example:
   `docker compose --env-file envFileMsSql.env -f docker-compose-mssql.yml --profile test up`.

The container copies the TRX file, Playwright HTML report, and failure artifacts to `test/output`.

# Comparing the legacy and Playwright suites locally

Install the Playwright dependencies as above, then install the frozen legacy dependencies:

```powershell
cd test/tests_e2e_legacy
corepack yarn@1.22.22 install --frozen-lockfile --ignore-scripts
cd ..
node run-e2e.js mssql
```

Use `postgresql` instead of `mssql` for that profile. The comparison launcher loads
`tests_e2e/.env.mssql` or `tests_e2e/.env.postgresql`, forces both runners into headless mode,
and runs legacy followed by Playwright. It runs the second suite even if the first fails,
then returns a failure exit code if either failed. Both suites reset shared test data, so
do not run them concurrently against the same application/database.

To run just one suite or selected files:

```powershell
node run-e2e.js mssql legacy
node run-e2e.js mssql playwright
node run-e2e.js mssql both 01_basicCrud.test.js
```

The launcher finds installed Chrome and a built `origam-utils.dll` automatically. To override
them, set `PUPPETEER_EXECUTABLE_PATH` and `ORIGAM_E2E_UTILS_PATH` in the active profile.
The .NET utility uses `OrigamSettings.config` beside its executable; ensure it targets the
same test database as the running application. Legacy reports are written inside
`tests_e2e_legacy`, and Playwright reports inside `tests_e2e`.

GitHub runs a separate job for each runner and database (four jobs in total), using the same
application image and model with an isolated server/database per job. Both authentication
phases are retained. Reports and diagnostic artifacts identify the runner and database.
Docker selects Playwright by default; set `ORIGAM_E2E_RUNNER=legacy` to select the old suite.
