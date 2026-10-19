/*
Copyright 2005 - 2022 Advantage Solutions, s. r. o.
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

const { defineConfig } = require("@playwright/test");
require("dotenv").config({ path: require("path").join(__dirname, ".env"), quiet: true });
const { backEndUrl } = require("./additionalConfig");

module.exports = defineConfig({
  testDir: ".",
  testMatch: "**/*.test.js",
  fullyParallel: false,
  workers: 1,
  timeout: 300_000,
  expect: { timeout: 20_000 },
  retries: process.env.CI ? 1 : 0,
  reporter: [
    ["list"],
    ["html", { outputFolder: "playwright-report", open: "never" }],
    ["./trxReporter.js", { outputFile: "frontend-integration-test-results.trx" }],
  ],
  outputDir: "test-results",
  use: {
    actionTimeout: 60_000,
    baseURL: backEndUrl,
    channel: "chrome",
    headless: (process.env.ORIGAM_E2E_HEADLESS ?? process.env.CI) === "true",
    ignoreHTTPSErrors: true,
    viewport: { width: 1800, height: 2000 },
    screenshot: "only-on-failure",
    trace: "on-first-retry",
    video: "on-first-retry",
    launchOptions: {
      args: process.env.CI
        ? ["--disable-dev-shm-usage", "--disable-setuid-sandbox", "--no-sandbox"]
        : [],
    },
  },
});
