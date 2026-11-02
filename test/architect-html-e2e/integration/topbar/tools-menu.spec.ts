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

test.describe.configure({ mode: 'serial' });

test.beforeEach(async ({ request }) => {
  await resetBackend(request);
});

test('groups tools in Menu and closes on Escape and outside clicks', async ({ page }) => {
  await page.goto('/');
  const menu = page.getByTestId('topbar-menu');
  const settings = page.getByTestId('topbar-settings');
  await expect(page.getByTestId('topbar-generate-guid')).toBeVisible();
  await expect(page.getByTestId('topbar-search-input')).toBeVisible();
  await expect(page.getByTestId('topbar-toggle-ai')).toBeVisible();
  await expect(settings).toBeHidden();

  await menu.focus();
  await page.keyboard.press('Enter');
  await expect(page.getByTestId('topbar-check-model')).toBeVisible();
  await expect(page.getByTestId('topbar-deployment-scripts')).toBeVisible();
  await expect(page.getByTestId('topbar-rebuild-localization')).toBeVisible();
  await expect(settings).toBeVisible();

  await page.keyboard.press('Escape');
  await expect(settings).toBeHidden();
  await expect(menu).toBeFocused();

  await menu.click();
  await page.mouse.click(1, 1);
  await expect(settings).toBeHidden();

  await menu.click();
  await settings.focus();
  await page.keyboard.press('Enter');
  await expect(page.getByRole('checkbox', { name: 'Vim mode' })).toBeVisible();
  await expect(settings).toBeHidden();
});

test('rebuilds localization files and reports the generated files', async ({ page }) => {
  await page.route('**/Localization/Rebuild', async route => {
    expect(route.request().method()).toBe('POST');
    await route.fulfill({ json: ['Root Menu-cs-CZ.xml', 'Root Menu-de-DE.xml'] });
  });
  await page.goto('/');
  await page.getByTestId('topbar-menu').click();
  await page.getByTestId('topbar-rebuild-localization').click();

  await expect(page.getByText('Localization files rebuilt', { exact: true })).toBeVisible();
  await expect(page.getByText('Root Menu-cs-CZ.xml, Root Menu-de-DE.xml')).toBeVisible();
  await expect(page.getByTestId('topbar-settings')).toBeHidden();
});

test('shows a localization rebuild error without reporting success', async ({ page }) => {
  await page.route('**/Localization/Rebuild', route =>
    route.fulfill({
      status: 420,
      json: {
        message: 'Configure TranslationBuilderLanguages before rebuilding localization files.',
      },
    }),
  );
  await page.goto('/');
  await page.getByTestId('topbar-menu').click();
  await page.getByTestId('topbar-rebuild-localization').click();

  await expect(
    page.getByText('Configure TranslationBuilderLanguages before rebuilding localization files.', {
      exact: true,
    }),
  ).toBeVisible();
  await expect(page.getByText('Localization files rebuilt', { exact: true })).toHaveCount(0);
});
