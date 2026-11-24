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

const XSL = `<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:template match="ROOT"><ROOT/></xsl:template>
</xsl:stylesheet>`;

const PREFIXED_XSL = `<?xml version="1.0" encoding="UTF-8"?>
<t:stylesheet xmlns:t="http://www.w3.org/1999/XSL/Transform" version="1.0">
<t:template match="ROOT"><ROOT/></t:template>
</t:stylesheet>`;

const XML_WITHOUT_XSL_NAMESPACE = `<?xml version="1.0" encoding="UTF-8"?>
<ROOT/>`;

function editorCodeArea(page: Page) {
  return page.getByTestId('code-editor').filter({ visible: true }).locator('.view-lines');
}

function focusedSuggestion(page: Page) {
  return page.locator('.suggest-widget .monaco-list-row.focused');
}

function noSuggestionsMessage(page: Page) {
  return page.locator('.suggest-widget').getByText('No suggestions.');
}

async function openTransformation(page: Page) {
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

async function openOnNewLine(page: Page, xsl: string) {
  await openTransformation(page);
  await replaceXsl(page, xsl);
  await page.keyboard.press('ControlOrMeta+End');
  await page.keyboard.press('Enter');
}

async function acceptSuggestion(page: Page, label: string) {
  await expect(focusedSuggestion(page)).toContainText(label);
  await page.keyboard.press('Enter');
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

test.describe('Tag closing and xsl completion in the XSLT editor (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Root');
  });

  test.afterEach(async ({ page }) => {
    await closeWithoutSaving(page);
  });

  test('closes a typed start tag and keeps the caret between the tags', async ({ page }) => {
    await openOnNewLine(page, XSL);

    await page.keyboard.type('<b class="x">');
    await page.keyboard.type('inside');

    await expect(editorCodeArea(page)).toContainText('<b class="x">inside</b>');
  });

  test('closes the tag at its last > and not at one inside an attribute value', async ({
    page,
  }) => {
    await openOnNewLine(page, XSL);

    await page.keyboard.type('<xsl:if test="a > b">');

    await expect(editorCodeArea(page)).toContainText('<xsl:if test="a > b"></xsl:if>');
  });

  test('leaves self-closing tags and comments without a closing tag', async ({ page }) => {
    await openOnNewLine(page, XSL);

    await page.keyboard.type('<br/><!-- note -->');

    await expect(editorCodeArea(page)).toContainText('<br/><!-- note -->');
    await expect(editorCodeArea(page)).not.toContainText('</br>');
  });

  test('first undo removes only the added closing tag', async ({ page }) => {
    await openOnNewLine(page, XSL);
    await page.keyboard.type('<u>');
    await expect(editorCodeArea(page)).toContainText('<u></u>');

    await page.keyboard.press('ControlOrMeta+z');

    await expect(editorCodeArea(page)).not.toContainText('<u></u>');
    await expect(editorCodeArea(page)).toContainText('<u>');
  });

  test('keeps text entered in one input event as it was entered', async ({ page }) => {
    await openOnNewLine(page, XSL);

    await page.keyboard.insertText('<items>\n<item/>\n</items>');

    await expect(editorCodeArea(page)).toContainText('<item/>');
    await expect(editorCodeArea(page)).not.toContainText('<items></items>');
  });

  test('completes an xsl element with its required attribute', async ({ page }) => {
    await openOnNewLine(page, XSL);

    await page.keyboard.type('<xsl:value-o');
    await acceptSuggestion(page, 'xsl:value-of');
    await page.keyboard.type('.');

    await expect(editorCodeArea(page)).toContainText('<xsl:value-of select="."/>');
  });

  test('Tab moves through the attributes of a completed element', async ({ page }) => {
    await openOnNewLine(page, XSL);

    await page.keyboard.type('<xsl:key');
    await acceptSuggestion(page, 'xsl:key');
    await page.keyboard.type('keyName');
    await page.keyboard.press('Tab');
    await page.keyboard.type('Row');
    await page.keyboard.press('Tab');
    await page.keyboard.type('@Id');

    await expect(editorCodeArea(page)).toContainText(
      '<xsl:key name="keyName" match="Row" use="@Id"/>',
    );
  });

  test('completes with the prefix the stylesheet binds to the XSL namespace', async ({ page }) => {
    await openOnNewLine(page, PREFIXED_XSL);

    await page.keyboard.type('<t:value-o');
    await acceptSuggestion(page, 't:value-of');
    await page.keyboard.type('.');

    await expect(editorCodeArea(page)).toContainText('<t:value-of select="."/>');
  });

  test('offers no xsl elements when the XSL namespace is not declared', async ({ page }) => {
    await openOnNewLine(page, XML_WITHOUT_XSL_NAMESPACE);

    await page.keyboard.type('<xsl:va');
    await page.keyboard.press('Control+Space');

    await expect(noSuggestionsMessage(page)).toBeVisible();
  });

  test('does not suggest words that already appear in the document', async ({ page }) => {
    await openOnNewLine(page, XSL);

    await page.keyboard.type('templ');
    await page.keyboard.press('Control+Space');

    await expect(noSuggestionsMessage(page)).toBeVisible();
  });
});
