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

const STACK_TRACE = 'System.NullReferenceException: boom\r\n   at Origam.Somewhere()';

test.describe('Server error dialog (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('a server error shows a short message instead of the stack trace', async ({ page }) => {
    await page.route('**/Model/GetMenuItems**', route =>
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify(STACK_TRACE),
      }),
    );
    await page.goto('/');
    await page.getByTestId('tree-toggle-Data').click();

    await page.getByTestId('tree-node-Entities').click({ button: 'right' });

    const dialog = page.locator('.dialogMessage');
    await expect(dialog).toContainText('Server error occurred. Please check server log');
    await expect(dialog).not.toContainText('NullReferenceException');
  });
});
