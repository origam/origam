/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { defineConfig } from '@playwright/test';
import path from 'node:path';
import { frontendWebServer, repoRoot, sharedConfig } from './playwright.config';

// Integration suite: runs the architect-html frontend against a real, running
// Origam.Architect.Server (which the Vite dev server proxies to). The backend
// reads the file model from model-tests/model and needs a reachable database
// (SQLEXPRESS / origam-demo locally, MSSQL LocalDB in CI).

// Where the backend listens. Must match the proxy targets in vite.config.ts.
const BACKEND_URL = process.env.ARCHITECT_SERVER_URL ?? 'https://localhost:7099';
const BACKEND_URLS_ARG = 'https://localhost:7099;http://localhost:5003';

// Directory of the built server (the OrigamSettings.config next to the exe
// points at the model + database). Defaults to the local Debug build; CI and
// custom setups override it, or override the whole command via ARCHITECT_SERVER_CMD.
const SERVER_DIR =
  process.env.ARCHITECT_SERVER_DIR ??
  path.join(repoRoot, 'backend/Origam.Architect.Server/bin/Debug/net8.0');
// Absolute path with native separators — cmd won't run a relative/forward-slash
// exe path, and cwd is not on PATH. The server reads OrigamSettings.config from
// next to its own exe, so cwd doesn't affect config loading.
const serverDir = path.resolve(process.cwd(), SERVER_DIR);
const serverExe = path.join(serverDir, 'Origam.Architect.Server.exe');
const SERVER_CMD =
  process.env.ARCHITECT_SERVER_CMD ?? `"${serverExe}" --urls "${BACKEND_URLS_ARG}"`;
// Working directory for the server process — also its ASP.NET content root, so
// it must contain appsettings.json. CI builds Release (not Debug), so it sets
// ARCHITECT_SERVER_CWD to the actual build dir. Must be an existing directory,
// otherwise the launcher fails with "spawn cmd.exe ENOENT".
const serverCwd = process.env.ARCHITECT_SERVER_CWD ?? serverDir;

export default defineConfig({
  ...sharedConfig,
  testDir: './integration',
  // Activating a package against a fresh database triggers a server-side deploy
  // (CI starts from an empty origam-demo), so allow generous per-test time.
  timeout: 120_000,
  webServer: [
    {
      // Start the real Architect Server. If you already run it in Visual Studio
      // (or CI started it), reuseExistingServer makes Playwright just wait for
      // it instead of launching a second instance.
      command: SERVER_CMD,
      cwd: serverCwd,
      env: { ASPNETCORE_ENVIRONMENT: 'Development' },
      // /Package/GetAll reads the file model and returns 200 without touching
      // the database, so it's a reliable readiness probe.
      url: `${BACKEND_URL}/Package/GetAll`,
      ignoreHTTPSErrors: true,
      reuseExistingServer: true,
      timeout: 180_000,
      stdout: 'pipe',
      stderr: 'pipe',
    },
    frontendWebServer,
  ],
});
