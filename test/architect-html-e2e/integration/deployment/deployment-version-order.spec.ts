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

import { randomUUID } from 'node:crypto';
import fs from 'node:fs';
import path from 'node:path';
import { expect, type Page, test } from '@playwright/test';
import { closeAiPanel } from '@support/aiPanel';
import { modelDirectory, reloadBackend, resetBackend } from '@support/resetBackend';

const versionDirectory = path.join(modelDirectory, 'Root Menu/DeploymentVersion/Root Menu');

function plantDeploymentVersion(version: string): void {
  const content = [
    '<?xml version="1.0" encoding="utf-8"?>',
    '<x:file',
    '  xmlns:x="http://schemas.origam.com/model-persistence/1.0.0"',
    '  xmlns:asi="http://schemas.origam.com/Origam.Schema.AbstractSchemaItem/6.0.0"',
    '  xmlns:dv="http://schemas.origam.com/Origam.Schema.DeploymentModel.DeploymentVersion/6.0.0">',
    '  <dv:DeploymentVersion',
    '    asi:abstract="false"',
    `    x:id="${randomUUID()}"`,
    `    asi:name="${version}"`,
    `    dv:version="${version}" />`,
    '</x:file>',
    '',
  ].join('\n');
  fs.writeFileSync(path.join(versionDirectory, `${version}.origam`), content, 'utf8');
}

async function openRootMenuVersions(page: Page): Promise<void> {
  await page.goto('/');
  await closeAiPanel(page);
  await page.getByTestId('tree-toggle-Common').click();
  await page.getByTestId('tree-toggle-Deployment').click();
  await page.getByTestId('tree-toggle-Root Menu').click();
}

async function expectTreeOrder(page: Page, nodeTexts: string[]): Promise<void> {
  await expect
    .poll(async () => {
      const testIds = await page
        .getByTestId(/^tree-node-/)
        .evaluateAll(nodes => nodes.map(node => node.getAttribute('data-test-id') ?? ''));
      return testIds
        .map(testId => testId.slice('tree-node-'.length))
        .filter(nodeText => nodeTexts.includes(nodeText));
    })
    .toEqual(nodeTexts);
}

test.describe('Deployment Version order in the model tree', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('orders versions by number, not alphabetically', async ({ page, request }) => {
    plantDeploymentVersion('1.9.0');
    plantDeploymentVersion('1.10.0');
    plantDeploymentVersion('1.22.0');
    await reloadBackend(request);

    await openRootMenuVersions(page);

    await expectTreeOrder(page, ['1.2.3', '1.3.0', '1.9.0', '1.10.0', '1.22.0']);
  });

  test('places a newly saved version by its number', async ({ page }) => {
    await openRootMenuVersions(page);
    await page.getByTestId('tree-node-Root Menu').click({ button: 'right' });
    await page.getByTestId('tree-menu-new').getByText('New').click();
    await page.getByText('Deployment Version').click();

    await page.getByRole('textbox', { name: 'NewDeploymentVersion' }).fill('1.10.0');
    await page
      .getByText('VersionString', { exact: true })
      .locator('xpath=../..')
      .getByRole('textbox')
      .fill('1.10.0');
    await page.getByTestId('save-button').click();
    await expect(page.getByTestId('save-button-disabled')).toBeVisible();

    await expectTreeOrder(page, ['1.2.3', '1.3.0', '1.10.0']);
  });
});
