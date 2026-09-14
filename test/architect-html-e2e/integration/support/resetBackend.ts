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

import { execFileSync } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';
import type { APIRequestContext } from '@playwright/test';
import { activatePackage } from '@support/activatePackage';

function findRepoRoot(): string {
  let dir = process.cwd();
  while (!fs.existsSync(path.join(dir, 'model-tests'))) {
    const parent = path.dirname(dir);
    if (parent === dir) {
      throw new Error('Could not locate the repository root (no model-tests directory above the current working directory).');
    }
    dir = parent;
  }
  return dir;
}

const repoRoot = findRepoRoot();
const MODEL_DIR = 'model-tests/model';
const DEFAULT_PACKAGE = 'Root Menu';

export function readModelFile(relativePath: string): string {
  return fs.readFileSync(path.join(repoRoot, MODEL_DIR, relativePath), 'utf8');
}

function runGit(args: string[]): void {
  const deadline = Date.now() + 30_000;
  for (;;) {
    try {
      execFileSync('git', args, { cwd: repoRoot, stdio: 'pipe' });
      return;
    } catch (error) {
      const message = String((error as { stderr?: Buffer }).stderr ?? (error as Error).message);
      if (!message.includes('index.lock') || Date.now() > deadline) {
        throw new Error(`git ${args.join(' ')} failed: ${message}`);
      }
      Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, 250);
    }
  }
}

export function restoreModelFiles(): void {
  runGit(['checkout', '--', MODEL_DIR]);
  runGit(['clean', '-fd', MODEL_DIR]);
}

async function postOrThrow(request: APIRequestContext, url: string): Promise<void> {
  const response = await request.post(url);
  if (!response.ok()) {
    throw new Error(`POST ${url} failed: ${response.status()} ${await response.text()}`);
  }
}

// Restoring the model files is bracketed by BeginReset/EndReset so the server
// stops reacting to file changes while git rewrites model-tests/model, and
// discards the events the restore produced instead of processing them a second
// later, in the middle of the test that follows.
export async function resetBackend(request: APIRequestContext): Promise<void> {
  await postOrThrow(request, '/Test/BeginReset');
  try {
    restoreModelFiles();
  } catch (error) {
    // Resume the queue, but report what git did rather than what EndReset said.
    await request.post('/Test/EndReset').catch(() => {});
    throw error;
  }
  await postOrThrow(request, '/Test/EndReset');

  await activatePackage(request, DEFAULT_PACKAGE);
}

export function modelFilePath(relativePath: string): string {
  return path.join(repoRoot, MODEL_DIR, relativePath);
}
