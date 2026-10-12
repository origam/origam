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
import fs from 'node:fs';
import { activatePackage } from '@support/activatePackage';
import { expectModelFile, openConstants } from '@support/modelTree';
import { modelFilePath, resetBackend } from '@support/resetBackend';
import { showPackagesTab } from '@support/showPackagesTab';

// resetBackend activates this one.
const DEFAULT_PACKAGE = 'Root Menu';
const OTHER_PACKAGE = 'Attachments';
// Every other package references it.
const REFERENCED_PACKAGE = 'Root';
const NEW_PACKAGE = 'E2EPackage';
const ROOT_REFERENCE = 'Root#147fa70d-6519-4393-b5d0-87931f9fd609';

const STRING_CONSTANT = 'DefaultMailWorkQueueName';

// References Root Menu, so Root Menu cannot reference it back.
const WIDGETS_PACKAGE = 'Widgets';
const WIDGETS_ID = 'f17329d6-3143-420a-a2e6-30e431eea51d';
const ATTACHMENTS_ID = 'bb8c67fb-44c1-4b41-8fce-4d50cd5a759d';
const ROOT_ID = '147fa70d-6519-4393-b5d0-87931f9fd609';

function packageItem(page: Page, name: string): Locator {
  return page.getByTestId(`package-${name}`);
}

async function openPackages(page: Page): Promise<void> {
  await page.goto('/');
  await showPackagesTab(page);
}

async function expectActive(page: Page, name: string): Promise<void> {
  await expect(packageItem(page, name)).toHaveAttribute('data-active', 'true');
}

function trackRequests(page: Page, urlPart: string): string[] {
  const urls: string[] = [];
  page.on('request', request => {
    if (request.url().includes(urlPart)) {
      urls.push(request.url());
    }
  });
  return urls;
}

async function activateByDoubleClick(page: Page, name: string): Promise<void> {
  const setActive = page.waitForResponse(response => response.url().includes('/Package/SetActive'));
  await packageItem(page, name).dblclick();
  expect((await setActive).ok()).toBeTruthy();
}

async function createPackage(page: Page, name: string): Promise<void> {
  await page.getByTestId('packages-add').click();
  await page.getByLabel('Package name').fill(name);
  const created = page.waitForResponse(response => response.url().includes('/Package/Create'));
  await page.getByRole('button', { name: 'OK' }).click();
  const response = await created;
  expect(response.ok(), await response.text()).toBeTruthy();
}

async function openReferences(page: Page, name: string): Promise<void> {
  await packageItem(page, name).click();
  const loaded = page.waitForResponse(response => response.url().includes('/Package/References'));
  await page.getByTestId(`package-references-${name}`).click();
  expect((await loaded).ok()).toBeTruthy();
}

function referenceCheckbox(page: Page, name: string): Locator {
  return page.getByTestId(`package-reference-${name}`);
}

async function saveReferences(page: Page): Promise<void> {
  const updated = page.waitForResponse(response =>
    response.url().includes('/Package/UpdateReferences'),
  );
  await page.getByRole('button', { name: 'OK' }).click();
  const response = await updated;
  expect(response.ok(), await response.text()).toBeTruthy();
}

function readPackageFile(name: string): string {
  return fs.readFileSync(modelFilePath(`${name}/.origamPackage`), 'utf8');
}

async function confirmDelete(page: Page): Promise<void> {
  const deleted = page.waitForResponse(response => response.url().includes('/Package/Delete'));
  await page.getByRole('button', { name: 'Yes' }).click();
  const response = await deleted;
  expect(response.ok(), await response.text()).toBeTruthy();
}

test.describe('Packages (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  // The next reset removes a created package from disk and then reloads the active package.
  test.afterEach(async ({ request }) => {
    await activatePackage(request, DEFAULT_PACKAGE);
  });

  test('loads the package list from the server', async ({ page }) => {
    const packagesLoaded = page.waitForResponse(
      response => response.url().includes('/Package/GetAll') && response.ok(),
    );
    await page.goto('/');
    await packagesLoaded;

    await showPackagesTab(page);
    await expect(page.getByText('Attachments', { exact: true })).toBeVisible();
    await expect(page.getByText('Audit', { exact: true })).toBeVisible();
  });

  test('a single click only selects a package', async ({ page }) => {
    await openPackages(page);
    const setActiveRequests = trackRequests(page, '/Package/SetActive');

    await packageItem(page, OTHER_PACKAGE).click();

    await expect(packageItem(page, OTHER_PACKAGE)).toHaveAttribute('data-selected', 'true');
    await expect(packageItem(page, OTHER_PACKAGE)).toHaveAttribute('data-active', 'false');
    await expectActive(page, DEFAULT_PACKAGE);
    expect(setActiveRequests).toHaveLength(0);
  });

  test('a double click activates a package', async ({ page }) => {
    await openPackages(page);

    await activateByDoubleClick(page, OTHER_PACKAGE);

    await expect(page.getByTestId('tree-toggle-Data')).toBeVisible();
    await showPackagesTab(page);
    await expectActive(page, OTHER_PACKAGE);
    await expect(packageItem(page, DEFAULT_PACKAGE)).toHaveAttribute('data-active', 'false');
  });

  test('offers deleting only the selected package', async ({ page }) => {
    await openPackages(page);
    await expect(page.locator('[data-test-id^="package-delete-"]')).toHaveCount(0);

    await packageItem(page, OTHER_PACKAGE).click();
    await expect(page.getByTestId(`package-delete-${OTHER_PACKAGE}`)).toBeVisible();

    await packageItem(page, DEFAULT_PACKAGE).click();
    await expect(page.getByTestId(`package-delete-${OTHER_PACKAGE}`)).toHaveCount(0);
    await expect(page.getByTestId(`package-delete-${DEFAULT_PACKAGE}`)).toBeVisible();
  });

  test('creates a package referencing Root and activates it', async ({ page }) => {
    await openPackages(page);

    await createPackage(page, NEW_PACKAGE);

    // The package gets a folder in every provider, Constants is one of them.
    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Constants').click();
    await expect(page.getByTestId(`tree-node-${NEW_PACKAGE}`)).toBeVisible();

    await showPackagesTab(page);
    await expectActive(page, NEW_PACKAGE);
    await expectModelFile(`${NEW_PACKAGE}/.origamPackage`, true);
    expect(fs.readFileSync(modelFilePath(`${NEW_PACKAGE}/.origamPackage`), 'utf8')).toContain(
      ROOT_REFERENCE,
    );
  });

  test('rejects an invalid package name', async ({ page }) => {
    await openPackages(page);
    await page.getByTestId('packages-add').click();
    const nameInput = page.getByLabel('Package name');
    const okButton = page.getByRole('button', { name: 'OK' });

    await expect(okButton).toBeDisabled();

    await nameInput.fill(OTHER_PACKAGE.toLowerCase());
    await expect(page.getByText('A package with this name already exists.')).toBeVisible();
    await expect(okButton).toBeDisabled();

    await nameInput.fill('bad/name');
    await expect(page.getByText('Package name contains invalid characters.')).toBeVisible();
    await expect(okButton).toBeDisabled();

    await nameInput.fill('CON');
    await expect(page.getByText('Package name is reserved or not allowed.')).toBeVisible();
    await expect(okButton).toBeDisabled();
  });

  test('cancelling the new package dialog creates nothing', async ({ page }) => {
    await openPackages(page);
    const createRequests = trackRequests(page, '/Package/Create');

    await page.getByTestId('packages-add').click();
    await page.getByLabel('Package name').fill(NEW_PACKAGE);
    await page.getByRole('button', { name: 'Cancel' }).click();

    await expect(page.getByLabel('Package name')).toHaveCount(0);
    await expect(packageItem(page, NEW_PACKAGE)).toHaveCount(0);
    await expectActive(page, DEFAULT_PACKAGE);
    expect(createRequests).toHaveLength(0);
  });

  test('deletes a selected inactive package', async ({ page }) => {
    await openPackages(page);
    await createPackage(page, NEW_PACKAGE);
    await showPackagesTab(page);
    await activateByDoubleClick(page, DEFAULT_PACKAGE);
    await showPackagesTab(page);

    await packageItem(page, NEW_PACKAGE).click();
    await page.getByTestId(`package-delete-${NEW_PACKAGE}`).click();
    await confirmDelete(page);

    await expect(packageItem(page, NEW_PACKAGE)).toHaveCount(0);
    await expectActive(page, DEFAULT_PACKAGE);
    await expectModelFile(`${NEW_PACKAGE}/.origamPackage`, false);
  });

  test('deletes the active package and leaves no package active', async ({ page, request }) => {
    await openPackages(page);
    await createPackage(page, NEW_PACKAGE);
    await showPackagesTab(page);

    await packageItem(page, NEW_PACKAGE).click();
    await page.getByTestId(`package-delete-${NEW_PACKAGE}`).click();
    await confirmDelete(page);

    await expect(packageItem(page, NEW_PACKAGE)).toHaveCount(0);
    await expect(page.locator('[data-active="true"]')).toHaveCount(0);
    await expectModelFile(`${NEW_PACKAGE}/.origamPackage`, false);
    const packagesInfo = await (await request.get('/Package/GetAll')).json();
    expect(packagesInfo.activePackageId).toBeNull();

    await page.getByText('Model', { exact: true }).click();
    await expect(page.locator('[data-test-id^="tree-toggle-"]')).toHaveCount(0);
  });

  test('keeps the package when the deletion is declined', async ({ page }) => {
    await openPackages(page);
    const deleteRequests = trackRequests(page, '/Package/Delete');

    await packageItem(page, OTHER_PACKAGE).click();
    await page.getByTestId(`package-delete-${OTHER_PACKAGE}`).click();
    await page.getByRole('button', { name: 'No' }).click();

    await expect(packageItem(page, OTHER_PACKAGE)).toBeVisible();
    expect(deleteRequests).toHaveLength(0);
  });

  test('refuses to delete a package other packages reference', async ({ page }) => {
    await openPackages(page);

    await packageItem(page, REFERENCED_PACKAGE).click();
    await page.getByTestId(`package-delete-${REFERENCED_PACKAGE}`).click();
    const deleted = page.waitForResponse(response => response.url().includes('/Package/Delete'));
    await page.getByRole('button', { name: 'Yes' }).click();
    expect((await deleted).status()).toBe(420);

    await expect(page.getByText('cannot be deleted because it is referenced by')).toBeVisible();
    await page.getByRole('button', { name: 'Ok', exact: true }).click();
    await expect(packageItem(page, REFERENCED_PACKAGE)).toBeVisible();
    await expectActive(page, DEFAULT_PACKAGE);
    await expectModelFile(`${REFERENCED_PACKAGE}/.origamPackage`, true);
  });

  test('cancelling the save prompt keeps the active package', async ({ page, request }) => {
    await activatePackage(request, REFERENCED_PACKAGE);
    const deleteRequests = trackRequests(page, '/Package/Delete');
    await openConstants(page);
    await page.getByTestId(`tree-node-${STRING_CONSTANT}`).dblclick();
    const valueInput = page.getByTestId('property-input-Value');
    await valueInput.fill('E2EQUEUE');
    await valueInput.press('Tab');
    await expect(page.getByTestId(`tab-dirty-${STRING_CONSTANT}`)).toBeVisible();

    await showPackagesTab(page);
    await packageItem(page, REFERENCED_PACKAGE).click();
    await page.getByTestId(`package-delete-${REFERENCED_PACKAGE}`).click();
    await page.getByRole('button', { name: 'Yes' }).click();
    await expect(page.getByText(`Do you want to save ${STRING_CONSTANT}?`)).toBeVisible();
    await page.getByRole('button', { name: 'Cancel' }).click();

    await expect(page.getByTestId(`tab-dirty-${STRING_CONSTANT}`)).toBeVisible();
    await expectActive(page, REFERENCED_PACKAGE);
    expect(deleteRequests).toHaveLength(0);
  });

  test('offers editing references only for the active package', async ({ page }) => {
    await openPackages(page);

    await packageItem(page, OTHER_PACKAGE).click();
    await expect(page.getByTestId(`package-references-${OTHER_PACKAGE}`)).toHaveCount(0);

    await packageItem(page, DEFAULT_PACKAGE).click();
    await expect(page.getByTestId(`package-references-${DEFAULT_PACKAGE}`)).toBeVisible();
  });

  test('adds a reference and shows the referenced package in the tree', async ({ page }) => {
    await openPackages(page);
    await createPackage(page, NEW_PACKAGE);
    await showPackagesTab(page);

    await openReferences(page, NEW_PACKAGE);
    await expect(referenceCheckbox(page, REFERENCED_PACKAGE)).toBeChecked();
    await referenceCheckbox(page, WIDGETS_PACKAGE).check();
    await saveReferences(page);

    expect(readPackageFile(NEW_PACKAGE)).toContain(WIDGETS_ID);
    await page.getByText('Model', { exact: true }).click();
    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Constants').click();
    await expect(page.getByTestId(`tree-node-${WIDGETS_PACKAGE}`)).toBeVisible();
  });

  test('removes an unused reference', async ({ page }) => {
    await openPackages(page);
    await createPackage(page, NEW_PACKAGE);
    await showPackagesTab(page);
    await openReferences(page, NEW_PACKAGE);
    await referenceCheckbox(page, OTHER_PACKAGE).check();
    await saveReferences(page);
    expect(readPackageFile(NEW_PACKAGE)).toContain(ATTACHMENTS_ID);

    await openReferences(page, NEW_PACKAGE);
    await referenceCheckbox(page, OTHER_PACKAGE).uncheck();
    await saveReferences(page);

    expect(readPackageFile(NEW_PACKAGE)).not.toContain(ATTACHMENTS_ID);
  });

  test('does not offer a reference that would create a cycle', async ({ page }) => {
    await openPackages(page);

    await openReferences(page, DEFAULT_PACKAGE);

    await expect(referenceCheckbox(page, REFERENCED_PACKAGE)).toBeChecked();
    await expect(referenceCheckbox(page, WIDGETS_PACKAGE)).toBeDisabled();
    await expect(page.getByText('would create a circular reference').first()).toBeVisible();
  });

  test('refuses to remove a reference that is still used', async ({ page, request }) => {
    await activatePackage(request, OTHER_PACKAGE);
    await openPackages(page);

    await openReferences(page, OTHER_PACKAGE);
    await referenceCheckbox(page, REFERENCED_PACKAGE).uncheck();
    const updated = page.waitForResponse(response =>
      response.url().includes('/Package/UpdateReferences'),
    );
    await page.getByRole('button', { name: 'OK' }).click();
    expect((await updated).status()).toBe(420);

    await expect(page.getByText('cannot be removed from the references because')).toBeVisible();
    await page.getByRole('button', { name: 'Ok', exact: true }).click();
    expect(readPackageFile(OTHER_PACKAGE)).toContain(ROOT_ID);
  });

  test('cancelling the references dialog changes nothing', async ({ page }) => {
    await openPackages(page);
    const updateRequests = trackRequests(page, '/Package/UpdateReferences');

    await openReferences(page, DEFAULT_PACKAGE);
    await referenceCheckbox(page, OTHER_PACKAGE).check();
    await page.getByRole('button', { name: 'Cancel' }).click();

    await expect(referenceCheckbox(page, OTHER_PACKAGE)).toHaveCount(0);
    expect(updateRequests).toHaveLength(0);
  });
});
