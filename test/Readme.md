# Running the end-to-end tests locally

The browser tests use Playwright and reset their test data directly through Node database drivers.

1. Start Origam with the automatic-test model deployed.
2. Set `OrigamSettings__DataConnectionString` to the application's SQL Server or PostgreSQL connection string.
3. Set `DatabaseType` to `mssql` or `postgresql`.
4. Optionally set `ORIGAM_E2E_URL`, `ORIGAM_E2E_USERNAME`, and `ORIGAM_E2E_PASSWORD`.
5. In `test/tests_e2e`, run `yarn install` and then `yarn test:e2e`.

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
