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

import { request, type APIRequestContext, type Page } from '@playwright/test';
import { spawn } from 'node:child_process';
import fs from 'node:fs';
import net from 'node:net';
import os from 'node:os';
import path from 'node:path';
import { serverCwd } from '../../playwright.integration.config';

const STARTUP_TIMEOUT = 180_000;

const API_ROOTS = new Set([
  '/Model',
  '/Package',
  '/Tab',
  '/Test',
  '/ScreenEditor',
  '/SectionEditor',
  '/PropertyEditor',
  '/Documentation',
  '/Xslt',
  '/Icons',
  '/DeploymentScripts',
  '/DeploymentScriptsGenerator',
  '/Search',
  '/ModelCheck',
  '/Localization',
  '/wizards',
  '/agent',
]);

export type ArchitectServer = {
  request: APIRequestContext;
  routePage: (page: Page) => Promise<void>;
  stop: () => Promise<void>;
};

export function createTemporaryDirectory(prefix: string): string {
  return fs.realpathSync.native(fs.mkdtempSync(path.join(os.tmpdir(), prefix)));
}

export function removeTemporaryDirectory(directory: string): void {
  fs.rmSync(directory, { recursive: true, force: true, maxRetries: 10 });
}

export async function startArchitectServer(modelDirectory: string): Promise<ArchitectServer> {
  const serverDirectory = createTemporaryDirectory('origam-architect-server-');
  fs.cpSync(serverCwd, serverDirectory, {
    recursive: true,
    filter: source => path.basename(source) !== 'logs',
  });
  pointSettingsToModel(serverDirectory, modelDirectory);

  const url = `http://localhost:${await findFreePort()}`;
  const serverProcess = spawn('dotnet', ['Origam.Architect.Server.dll', '--urls', url], {
    cwd: serverDirectory,
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: 'Development' },
  });
  let output = '';
  serverProcess.stdout.on('data', chunk => (output += chunk));
  serverProcess.stderr.on('data', chunk => (output += chunk));
  const exited = new Promise<void>(resolve => serverProcess.once('exit', () => resolve()));
  const serverRequest = await request.newContext({ baseURL: url });

  const stop = async () => {
    await serverRequest.dispose();
    if (serverProcess.exitCode === null) {
      serverProcess.kill();
      await exited;
    }
    removeTemporaryDirectory(serverDirectory);
  };

  try {
    await waitUntilReady(serverRequest, () => serverProcess.exitCode !== null);
  } catch (error) {
    await stop();
    throw new Error(`${(error as Error).message}\n${output}`);
  }

  return {
    request: serverRequest,
    routePage: async page => {
      await page.route(
        requestUrl => API_ROOTS.has(`/${requestUrl.pathname.split('/')[1]}`),
        async route => {
          const pageRequest = route.request();
          const { pathname, search } = new URL(pageRequest.url());
          const response = await serverRequest.fetch(`${url}${pathname}${search}`, {
            method: pageRequest.method(),
            headers: pageRequest.headers(),
            data: pageRequest.postDataBuffer() ?? undefined,
          });
          await route.fulfill({ response });
        },
      );
    },
    stop,
  };
}

function pointSettingsToModel(serverDirectory: string, modelDirectory: string): void {
  const settingsPath = path.join(serverDirectory, 'OrigamSettings.config');
  const settings = fs.readFileSync(settingsPath, 'utf8');
  const modelLocation = /<ModelSourceControlLocation>[^<]*<\/ModelSourceControlLocation>/;
  if (!modelLocation.test(settings)) {
    throw new Error(`${settingsPath} does not set ModelSourceControlLocation.`);
  }
  fs.writeFileSync(
    settingsPath,
    settings.replace(
      modelLocation,
      `<ModelSourceControlLocation>${modelDirectory}</ModelSourceControlLocation>`,
    ),
  );
}

function findFreePort(): Promise<number> {
  return new Promise((resolve, reject) => {
    const probe = net.createServer();
    probe.once('error', reject);
    probe.listen(0, 'localhost', () => {
      const { port } = probe.address() as net.AddressInfo;
      probe.close(() => resolve(port));
    });
  });
}

async function waitUntilReady(
  serverRequest: APIRequestContext,
  hasExited: () => boolean,
): Promise<void> {
  const deadline = Date.now() + STARTUP_TIMEOUT;
  while (Date.now() < deadline) {
    if (hasExited()) {
      throw new Error('The Architect server exited during startup.');
    }
    const isReady = await serverRequest
      .get('/Package/GetAll', { timeout: 10_000 })
      .then(response => response.ok())
      .catch(() => false);
    if (isReady) {
      return;
    }
    await new Promise(resolve => setTimeout(resolve, 1_000));
  }
  throw new Error(`The Architect server did not start within ${STARTUP_TIMEOUT} ms.`);
}
