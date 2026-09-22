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

import fs from 'node:fs';
import path from 'node:path';
import { findRepoRoot, MODEL_DIR } from '@support/resetBackend';

const PROPERTY_BINDING_TAG = /<[A-Za-z0-9_]+:PropertyBindingInfo\b[^>]*>/g;
const VALUE_ATTRIBUTE = /\s[A-Za-z0-9_]+:value=/;

export interface PropertyBindingScan {
  offenders: string[];
  bindingsScanned: number;
}

function collectModelFiles(directory: string, collected: string[]): void {
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    const entryPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      collectModelFiles(entryPath, collected);
    } else if (entry.name.endsWith('.origam')) {
      collected.push(entryPath);
    }
  }
}

export function scanPropertyBindings(): PropertyBindingScan {
  const repoRoot = findRepoRoot();
  const modelFiles: string[] = [];
  collectModelFiles(path.join(repoRoot, MODEL_DIR), modelFiles);

  const offenders: string[] = [];
  let bindingsScanned = 0;
  for (const modelFile of modelFiles) {
    const content = fs.readFileSync(modelFile, 'utf8');
    for (const binding of content.match(PROPERTY_BINDING_TAG) ?? []) {
      bindingsScanned += 1;
      if (!VALUE_ATTRIBUTE.test(binding)) {
        offenders.push(`${path.relative(repoRoot, modelFile)}: ${binding.replace(/\s+/g, ' ')}`);
      }
    }
  }
  return { offenders, bindingsScanned };
}
