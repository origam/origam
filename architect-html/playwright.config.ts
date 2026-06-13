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

import { defineConfig, devices, type PlaywrightTestConfig } from '@playwright/test';

// The Vite dev server runs over HTTPS (vite-plugin-mkcert) on port 5173 and
// proxies API calls to Origam.Architect.Server. We pin the port so the
// webServer URL is deterministic in CI. Override the base URL via
// PLAYWRIGHT_BASE_URL to point the suite at an already-running server.
export const PORT = 5173;
// mkcert can't provision a local CA on CI runners, so the HTTPS dev server
// never comes up there. Run the dev server over plain HTTP in CI (and whenever
// PW_HTTP=true) and keep HTTPS for local dev. VITE_DISABLE_HTTPS is read by
// vite.config.ts to drop the https/mkcert setup.
const useHttp = !!process.env.CI || process.env.PW_HTTP === 'true';
const protocol = useHttp ? 'http' : 'https';
export const baseURL = process.env.PLAYWRIGHT_BASE_URL ?? `${protocol}://localhost:${PORT}`;

// The frontend dev server. Reused by the integration config, which adds the
// backend server in front of it.
export const frontendWebServer: NonNullable<PlaywrightTestConfig['webServer']> = {
  command: `yarn dev --port ${PORT} --strictPort`,
  env: useHttp ? { VITE_DISABLE_HTTPS: 'true' } : {},
  url: baseURL,
  ignoreHTTPSErrors: true,
  reuseExistingServer: !process.env.CI,
  timeout: 120_000,
};

// Settings shared between the smoke config (this file) and the integration
// config (playwright.integration.config.ts).
export const sharedConfig: PlaywrightTestConfig = {
  fullyParallel: true,
  // Fail the build if a test was accidentally committed with test.only.
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: process.env.CI
    ? [['html', { open: 'never' }], ['github'], ['list']]
    : [['html', { open: 'never' }], ['list']],
  use: {
    baseURL,
    // The mkcert / ASP.NET dev certificates are self-signed; trust them.
    ignoreHTTPSErrors: true,
    // Capture a full trace (actions, network, DOM snapshots) when a test is
    // retried after failing. Open it with `yarn test:e2e:report` or
    // `npx playwright show-trace <trace.zip>`.
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
};

// Smoke suite: only touches the app shell, so it needs no backend and runs in
// the lightweight CI job. Integration tests live in playwright.integration.config.ts.
export default defineConfig({
  ...sharedConfig,
  testDir: './e2e/smoke',
  webServer: frontendWebServer,
});
