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

import { expect, test } from '@playwright/test';
import { resetBackend } from '@support/resetBackend';

test.describe('Show SQL for data structure (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('Show SQL renders the Dimension1 SELECT statement', async ({ page }) => {
    await page.goto('/');

    await page.getByText('Root Menu').click();
    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Data Structures').click();
    await page.getByTestId('tree-toggle-Dimensions').click();
    await page.getByTestId('tree-node-Dimension1').click({ button: 'right' });
    await page.getByText('Actions', { exact: true }).click();
    await page.getByText('Show SQL').click();

    const code = page.getByRole('code');
    await expect(code).toContainText('SELECT');
    await expect(code).toContainText(
      '[Dimension1].[refDimensionTypeId] AS [refDimensionTypeId],',
    );
    await expect(code).toContainText('[Dimension1].[Name] AS [Name],');
    await expect(code).toContainText(
      '-- SQL statements for data structure: Dimension1' +
        '------------------------------------------------------------------- Dimension1' +
        '-----------------------------------------------------------------SELECT ' +
        '[Dimension1].[refDimensionTypeId] AS [refDimensionTypeId], ' +
        '[Dimension1].[Name] AS [Name], [Dimension1].[IsActive] AS [IsActive], ' +
        '[Dimension1].[RecordCreated] AS [RecordCreated], ' +
        '[Dimension1].[RecordUpdatedBy] AS [RecordUpdatedBy], ' +
        '[Dimension1].[Id] AS [Id], [Dimension1].[RecordCreatedBy] AS [RecordCreatedBy], ' +
        '[Dimension1].[RecordUpdated] AS [RecordUpdated]FROM [Dimension1] AS [Dimension1];',
    );

    await expect(page.getByText('SQL: Dimension1').nth(1)).toBeVisible();
  });
});
