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
import { type APIRequestContext, expect, type Locator, type Page, test } from '@playwright/test';
import { closeAiPanel } from '@support/aiPanel';
import { menuItem, openContextMenu } from '@support/modelTree';
import { modelDirectory, reloadBackend, resetBackend } from '@support/resetBackend';

const VERSION = '9.8.7';
const PLATFORMS = ['MsSql', 'PgSql'];
const DATA_SERVICE =
  'Root/Service/DataService.origam#DataService/bbd7bd32-d40b-441a-bb5b-0b0fa89169d4';
const versionDirectory = path.join(modelDirectory, 'Root Menu/DeploymentVersion/Root Menu');

type Script = { name: string; platform: string; sql: string };

function plantScript(script: Script): void {
  const versionFile = `${VERSION}.origam`;
  const scriptId = randomUUID();
  const commandTextFile = `${versionFile}___commandText___${scriptId}.txt`;
  fs.writeFileSync(path.join(versionDirectory, commandTextFile), script.sql, 'utf8');
  const content = [
    '<?xml version="1.0" encoding="utf-8"?>',
    '<x:file',
    '  xmlns:x="http://schemas.origam.com/model-persistence/1.0.0"',
    '  xmlns:asi="http://schemas.origam.com/Origam.Schema.AbstractSchemaItem/6.0.0"',
    '  xmlns:ausa="http://schemas.origam.com/Origam.Schema.DeploymentModel.AbstractUpdateScriptActivity/6.0.0"',
    '  xmlns:dv="http://schemas.origam.com/Origam.Schema.DeploymentModel.DeploymentVersion/6.0.0"',
    '  xmlns:scusa="http://schemas.origam.com/Origam.Schema.DeploymentModel.ServiceCommandUpdateScriptActivity/6.0.0">',
    '  <dv:DeploymentVersion',
    '    asi:abstract="false"',
    `    x:id="${randomUUID()}"`,
    `    asi:name="${VERSION}"`,
    `    dv:version="${VERSION}">`,
    '    <scusa:DeploymentUpdateScriptActivity',
    '      asi:abstract="false"',
    '      ausa:activityOrder="10"',
    `      scusa:commandText="-***-ExternalFile:${commandTextFile}"`,
    `      x:id="${scriptId}"`,
    `      asi:name="${script.name}"`,
    `      scusa:platform="${script.platform}"`,
    `      scusa:service="${DATA_SERVICE}" />`,
    '  </dv:DeploymentVersion>',
    '</x:file>',
    '',
  ].join('\n');
  fs.writeFileSync(path.join(versionDirectory, versionFile), content, 'utf8');
}

async function configuredPlatforms(request: APIRequestContext): Promise<string[]> {
  const response = await request.get('/Test/Platforms');
  expect(response.ok(), await response.text()).toBeTruthy();
  return (await response.json()) as string[];
}

async function executeScript(page: Page, script: Script): Promise<void> {
  const nodeText = `${script.name} (${script.platform})`;
  await page.goto('/');
  await closeAiPanel(page);
  await page.getByTestId('tree-toggle-Common').click();
  await page.getByTestId('tree-toggle-Deployment').click();
  await page.getByTestId('tree-toggle-Root Menu').click();
  await page.getByTestId(`tree-toggle-${VERSION}`).click();
  await openContextMenu(page, nodeText);
  await menuItem(page, nodeText, 'tree-menu-run-update-script-activity').click();
}

function executedNotification(page: Page): Locator {
  return page.getByRole('status').filter({ hasText: 'Deployment script executed' });
}

test.describe('Deployment script Execute (real backend)', () => {
  let platform: string;

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    [platform] = await configuredPlatforms(request);
  });

  test('shows that the script ran and its output', async ({ page, request }) => {
    const script = {
      name: '00010_E2E_CreateAndDropTable',
      platform,
      sql: 'CREATE TABLE e2e_execute_check (id int);\nDROP TABLE e2e_execute_check;',
    };
    plantScript(script);
    await reloadBackend(request);

    await executeScript(page, script);

    const notification = executedNotification(page);
    await expect(notification).toContainText(`${script.name} (${platform})`);
    await expect(notification.locator('pre')).toHaveText('0 records affected.');
  });

  test('shows the rows the script returns', async ({ page, request }) => {
    const script = { name: '00010_E2E_SelectRow', platform, sql: 'SELECT 1 AS e2e_value' };
    plantScript(script);
    await reloadBackend(request);

    await executeScript(page, script);

    const output = executedNotification(page).locator('pre');
    await expect(output).toContainText('e2e_value');
    await expect(output).toContainText('1 record affected.');
  });

  test('shows the database error when the script fails', async ({ page, request }) => {
    const script = {
      name: '00010_E2E_MissingTable',
      platform,
      sql: 'SELECT * FROM e2e_missing_table',
    };
    plantScript(script);
    await reloadBackend(request);

    await executeScript(page, script);

    const errorMessage = page.locator('.dialogMessage');
    await expect(errorMessage).toContainText(`Deployment script ${script.name} failed:`);
    await expect(errorMessage).toContainText('e2e_missing_table');
    await expect(executedNotification(page)).toHaveCount(0, { timeout: 1_000 });
  });

  test('reports a platform that is not configured', async ({ page, request }) => {
    const platforms = await configuredPlatforms(request);
    const missingPlatform = PLATFORMS.find(candidate => !platforms.includes(candidate));
    test.skip(!missingPlatform, 'Every platform is configured on this server.');
    const script = { name: '00010_E2E_OtherPlatform', platform: missingPlatform!, sql: 'SELECT 1' };
    plantScript(script);
    await reloadBackend(request);

    await executeScript(page, script);

    await expect(page.locator('.dialogMessage')).toContainText(
      `Deployment script ${script.name} was not executed because platform ${missingPlatform} is not configured.`,
    );
    await expect(executedNotification(page)).toHaveCount(0, { timeout: 1_000 });
  });
});
