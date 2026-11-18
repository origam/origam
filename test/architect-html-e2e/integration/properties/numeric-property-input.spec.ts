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

import { expect, test, type Locator, type Page } from '@playwright/test';
import { activatePackage } from '@support/activatePackage';
import { closeAiPanel } from '@support/aiPanel';
import { awaitPropertyUpdate } from '@support/propertyEditor';
import { resetBackend } from '@support/resetBackend';

const TEXT_FIELD_ID = '8954ae10-e0db-49b8-b04c-351e03249c36';

test.describe('Numeric property input (real backend)', () => {
  test.use({ actionTimeout: 10_000, navigationTimeout: 20_000 });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
  });

  test('keeps the digits typed after the previous value was sent', async ({ page, request }) => {
    const response = await request.post('/Tab/Open', { data: { schemaItemId: TEXT_FIELD_ID } });
    expect(response.ok(), await response.text()).toBeTruthy();
    await page.goto('/');
    await closeAiPanel(page);
    const input = dataLengthInput(page);
    await expect(input).toHaveValue('50');

    await input.press('End');
    await awaitPropertyUpdate(page, () => input.press('0'));
    await expect(input).toHaveValue('500');
    await awaitPropertyUpdate(page, () => input.press('0'));
    await expect(input).toHaveValue('5000');
    await awaitPropertyUpdate(page, () => input.press('Backspace'));
    await expect(input).toHaveValue('500');
  });
});

function dataLengthInput(page: Page): Locator {
  return page.getByTestId('property-label-DataLength').locator('..').locator('input');
}
