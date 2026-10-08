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

import { expect, test, type Page } from '@playwright/test';
import { activatePackage } from '@support/activatePackage';
import { resetBackend } from '@support/resetBackend';

const TRANSFORMATION = 'SD_Empty';
const FIRST_LINE = '<?xml version="1.0" encoding="UTF-8"?>';
const NOTIFICATION_DURATION_MS = 11_250;
const NOTIFICATION_TIMING_MARGIN_MS = 3_000;

const VALID_XSLT = `<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:template match="ROOT"><ROOT/></xsl:template>
</xsl:stylesheet>`;

const INVALID_XPATH_XSLT = `<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:template match="ROOT"><ROOT><xsl:value-of select="1 +"/></ROOT></xsl:template>
</xsl:stylesheet>`;

const MALFORMED_XML_XSLT = `<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:template match="ROOT"><ROOT></xsl:template>
</xsl:stylesheet>`;

const NODE_SET_PARAMETER = 'config';

const TYPED_NODE_SET_PARAMETER_XSLT = `<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
  xmlns:AS="http://schema.advantages.cz/AsapFunctions">
<xsl:param name="${NODE_SET_PARAMETER}" AS:DataType="Xml"/>
<xsl:template match="ROOT"><ROOT><xsl:value-of select="count($${NODE_SET_PARAMETER}/ROOT/Item)"/></ROOT></xsl:template>
</xsl:stylesheet>`;

const UNTYPED_NODE_SET_PARAMETER_XSLT = `<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:param name="${NODE_SET_PARAMETER}"/>
<xsl:template match="ROOT"><ROOT><xsl:value-of select="count($${NODE_SET_PARAMETER}/ROOT/Item)"/></ROOT></xsl:template>
</xsl:stylesheet>`;

function editorCodeArea(page: Page) {
  return page.getByTestId('code-editor').first().locator('.view-lines');
}

function successToast(page: Page) {
  return page.getByRole('status').filter({ hasText: 'XSLT is valid' });
}

function failureToast(page: Page) {
  return page.getByRole('status').filter({ hasText: 'XSLT validation failed' });
}

async function openTransformation(page: Page) {
  await page.clock.install();
  await page.goto('/');
  await page.getByTestId('tree-toggle-Business Logic').click();
  await page.getByTestId('tree-toggle-Transformations').click();
  await page.getByTestId(`tree-node-${TRANSFORMATION}`).dblclick();
  await expect(editorCodeArea(page)).toContainText(FIRST_LINE, { timeout: 30_000 });
}

async function replaceXsl(page: Page, xsl: string) {
  await page.context().grantPermissions(['clipboard-read', 'clipboard-write']);
  await page.evaluate(text => navigator.clipboard.writeText(text), xsl);
  await editorCodeArea(page).click();
  await page.keyboard.press('ControlOrMeta+a');
  const propertyUpdated = page.waitForResponse(
    response => response.url().includes('/PropertyEditor/Update') && response.ok(),
  );
  await page.keyboard.press('ControlOrMeta+v');
  await propertyUpdated;
}

async function closeWithoutSaving(page: Page) {
  const closeButton = page.getByTestId(`tab-close-${TRANSFORMATION}`);
  if ((await closeButton.count()) === 0) {
    return;
  }
  await closeButton.click();
  const dontSave = page.getByRole('button', { name: 'No' });
  const dialogShown = await dontSave.waitFor({ timeout: 2_000 }).then(
    () => true,
    () => false,
  );
  if (dialogShown) {
    await dontSave.click();
  }
  await expect(page.getByTestId(`tab-${TRANSFORMATION}`)).toHaveCount(0);
}

async function validate(page: Page) {
  const validated = page.waitForResponse(
    response => response.url().includes('/Xslt/Validate') && response.ok(),
  );
  await page.getByText('Validate', { exact: true }).filter({ visible: true }).click();
  await validated;
}

async function openInputParameters(page: Page) {
  const parametersLoaded = page.waitForResponse(
    response => response.url().includes('/Xslt/Parameters') && response.ok(),
  );
  await page.getByText('Input Parameters', { exact: true }).click();
  await parametersLoaded;
}

function parameterTypeSelect(page: Page, parameterName: string) {
  return page.getByText(parameterName, { exact: true }).locator('xpath=following-sibling::select');
}

test.describe('XSLT validation result (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Root');
  });

  test.afterEach(async ({ page }) => {
    await closeWithoutSaving(page);
  });

  test('valid XSLT shows a toast that closes by itself', async ({ page }) => {
    await openTransformation(page);

    await validate(page);

    await expect(successToast(page)).toBeVisible();
    await expect(successToast(page)).toContainText(`${TRANSFORMATION} passed validation`);
    await expect(page.getByRole('button', { name: 'OK' })).toHaveCount(0);
    await expect(page.getByText('XSLT is valid.', { exact: true })).toBeVisible();

    await page.clock.fastForward(NOTIFICATION_DURATION_MS - NOTIFICATION_TIMING_MARGIN_MS);
    await expect(successToast(page)).toBeVisible();
    await page.clock.fastForward(NOTIFICATION_TIMING_MARGIN_MS + 1_000);
    await expect(successToast(page)).toHaveCount(0);
  });

  test('XSLT that fails to transform shows a toast that closes by itself', async ({ page }) => {
    await openTransformation(page);
    await replaceXsl(page, INVALID_XPATH_XSLT);

    await validate(page);

    await expect(failureToast(page)).toBeVisible();
    await expect(failureToast(page)).toContainText('See the output for details');
    await expect(successToast(page)).toHaveCount(0);
    await expect(page.getByText("Unexpected token '<eof>' in the expression.")).toBeVisible();

    await page.clock.fastForward(NOTIFICATION_DURATION_MS - NOTIFICATION_TIMING_MARGIN_MS);
    await expect(failureToast(page)).toBeVisible();
    await page.clock.fastForward(NOTIFICATION_TIMING_MARGIN_MS + 1_000);
    await expect(failureToast(page)).toHaveCount(0);
  });

  test('a new validation result replaces the previous toast', async ({ page }) => {
    await openTransformation(page);
    await replaceXsl(page, MALFORMED_XML_XSLT);

    await validate(page);
    await expect(failureToast(page)).toBeVisible();
    const firstFailureToast = await failureToast(page).elementHandle();
    await validate(page);
    await expect
      .poll(() => firstFailureToast!.evaluate(element => element.isConnected))
      .toBe(false);
    await expect(failureToast(page)).toHaveCount(1);

    await replaceXsl(page, VALID_XSLT);
    await validate(page);

    await expect(successToast(page)).toBeVisible();
    await expect(failureToast(page)).toHaveCount(0);
    await expect(page.getByRole('status')).toHaveCount(1);
  });

  test('a parameter declared as Xml is validated as a node-set', async ({ page }) => {
    await openTransformation(page);
    await replaceXsl(page, TYPED_NODE_SET_PARAMETER_XSLT);

    await validate(page);
    await expect(successToast(page)).toBeVisible();
    await successToast(page).getByRole('button', { name: 'Dismiss' }).click();

    await openInputParameters(page);
    await expect(parameterTypeSelect(page, NODE_SET_PARAMETER)).toHaveValue('Xml');
    await validate(page);
    await expect(successToast(page)).toBeVisible();
  });

  test('validation uses the parameter type chosen in Input Parameters', async ({ page }) => {
    await openTransformation(page);
    await replaceXsl(page, UNTYPED_NODE_SET_PARAMETER_XSLT);

    await validate(page);
    await expect(failureToast(page)).toBeVisible();

    await openInputParameters(page);
    await parameterTypeSelect(page, NODE_SET_PARAMETER).selectOption('Xml');
    await validate(page);
    await expect(successToast(page)).toBeVisible();
    await expect(failureToast(page)).toHaveCount(0);
  });
});
