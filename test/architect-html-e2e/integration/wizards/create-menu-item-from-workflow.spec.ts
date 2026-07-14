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

test.describe('Create Menu Item from Sequential Workflow wizard (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('Create Menu Item from a workflow with a role', async ({ page }) => {
    await page.goto('/');

    await page.getByTestId('tree-toggle-Business Logic').click();
    await page.getByTestId('tree-toggle-Sequential Workflows').click();
    await page.getByTestId('tree-toggle-Widgets').click();
    await page.getByTestId('tree-node-DummyWorkflowDoingNothing').click({ button: 'right' });
    await page.getByText('Actions', { exact: true }).click();
    await page.getByText('Create Menu Item').click();

    const dialog = page.getByRole('dialog');
    await expect(dialog).toContainText('Create Menu from Sequential Workflow');

    await page
      .getByRole('textbox', { name: 'e.g. DummyWorkflowDoingNothing' })
      .fill('WorkflowMenuCaption');

    await page.getByRole('button', { name: 'Next →' }).click();
    await expect(dialog).toContainText('Sequential Workflow Reference');

    await page.getByRole('button', { name: 'Create Menu Item' }).click();

    await page.getByRole('button', { name: 'Show result' }).click();
    await expect(page.locator('tbody')).toContainText('Menu\\DummyWorkflowDoingNothing');
    await expect(page.locator('#root')).toContainText('Search results for "Menu Item"');
  });
});
