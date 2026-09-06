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
import { modelDirectory } from '@support/resetBackend';

// Plants defects for the validator to report; resetBackend() restores them after each test.

// Well-formed guids that are deliberately not in the model.
export const missingId = 'deadbeef-0000-0000-0000-000000000001';
export const missingGroupId = 'deadbeef-0000-0000-0000-000000000002';

const widgetsPackageId = 'f17329d6-3143-420a-a2e6-30e431eea51d';

// Over the 128 character identifier limit of SQL Server, and over the 63 of
// Postgres, so the rule fires on either database.
const overlongIdentifier = 'x'.repeat(200);

const deploymentVersionFile = path.join(
  modelDirectory,
  'AutomaticTests/DeploymentVersion/AutomaticTests/1.0.3.origam',
);
const entityWithColumnsFile = path.join(
  modelDirectory,
  'Widgets/DataEntity/Widgets/ArrayTest.origam',
);
const entityDirectory = path.join(modelDirectory, 'Widgets/DataEntity/Widgets');
const orphanFileDirectory = path.join(
  modelDirectory,
  'AutomaticTests/DataEntity/AutomaticTests',
);

function replaceOnce(filePath: string, search: string, replacement: string): void {
  const content = fs.readFileSync(filePath, 'utf8');
  if (!content.includes(search)) {
    throw new Error(`"${search}" was not found in ${filePath}. The test model has changed.`);
  }
  fs.writeFileSync(filePath, content.replace(search, replacement), 'utf8');
}

// Points a deployment activity at a service that does not exist, which the
// "Invalid References Between Origam Files" check reports.
export function plantMissingReference(): { instanceId: string } {
  replaceOnce(deploymentVersionFile, 'bbd7bd32-d40b-441a-bb5b-0b0fa89169d4', missingId);
  return { instanceId: '7400b485-8bbc-4585-8d8f-36561433cd22' };
}

// Makes a database column name longer than the database allows, which the
// LengthLimit model rule reports.
export function plantOverlongColumnName(): { itemPath: string } {
  replaceOnce(
    entityWithColumnsFile,
    'fmi:mappedColumnName="Text"',
    `fmi:mappedColumnName="${overlongIdentifier}"`,
  );
  return { itemPath: 'ArrayTest\\Text' };
}

// Adds a file no model element points at, which the dead model files check
// reports.
export function plantOrphanFile(): { fileName: string } {
  const fileName = 'orphan-notes.txt';
  fs.writeFileSync(
    path.join(orphanFileDirectory, fileName),
    'Not referenced by any model element.\n',
    'utf8',
  );
  return { fileName };
}

// Moves an object into a file named after something else, which the file name
// check reports.
export function plantMisnamedFile(): { expectedFileName: string; actualFileName: string } {
  const expectedFileName = 'TagInputSource.origam';
  const actualFileName = 'MisnamedSource.origam';
  fs.renameSync(
    path.join(entityDirectory, expectedFileName),
    path.join(entityDirectory, actualFileName),
  );
  return { expectedFileName, actualFileName };
}

// A group reference file overrides .origamGroup, leaving every item in the folder with an unresolved group id.
export function plantBrokenGroupReference(): { missingGroupId: string } {
  const content = [
    '<?xml version="1.0" encoding="utf-8"?>',
    '<x:file xmlns:x="http://schemas.origam.com/model-persistence/1.0.0">',
    `    <x:groupReference x:type="package" x:refId="${widgetsPackageId}"/>`,
    `    <x:groupReference x:type="group" x:refId="${missingGroupId}"/>`,
    '</x:file>',
    '',
  ].join('\n');
  fs.writeFileSync(path.join(entityDirectory, '.origamGroupReference'), content, 'utf8');
  return { missingGroupId };
}
