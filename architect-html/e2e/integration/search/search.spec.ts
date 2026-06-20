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
import { resetBackend } from '../support/resetBackend';

test.describe('Search virtual entity test', () => {
    test.beforeEach(async ({ request }) => {
        await resetBackend(request);
    });

    test('Database Entity creation', async ({ page }) => {
        await page.goto('/');

        await page.getByTestId('topbar-search-input').click();
        await page.getByTestId('topbar-search-input').fill('Iactive');
        await expect(page.locator('tbody')).toContainText('IActive');
        await page.getByRole('cell', { name: 'Virtual Entity' }).first().click();
        await expect(page.locator('tbody')).toContainText('Virtual Entity');
    });
});
