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

export function restoreModelFiles(): void {
  execFileSync('git', ['checkout', '--', MODEL_DIR], { cwd: repoRoot, stdio: 'pipe' });
  execFileSync('git', ['clean', '-fd', MODEL_DIR], { cwd: repoRoot, stdio: 'pipe' });
}

export async function resetBackend(request: APIRequestContext): Promise<void> {
  restoreModelFiles();

  const response = await request.post('/Test/Reset');
  if (!response.ok()) {
    throw new Error(`POST /Test/Reset failed: ${response.status()} ${await response.text()}`);
  }
}
