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
import { resetBackend } from '@support/resetBackend';

const FIRST_LINE = '<?xml version="1.0" encoding="UTF-8"?>';

const cases = [
  {
    caption: 'XSL Transformation',
    parentNode: 'Transformations',
    typeName: 'Origam.Schema.EntityModel.XslTransformation',
    expectedLines: ['<xsl:template match="ROOT">', '<xsl:copy-of select="@*"/>'],
  },
  {
    caption: 'Validation Rule',
    parentNode: 'Rules',
    typeName: 'Origam.Schema.RuleModel.EndRule',
    expectedLines: ['<RuleExceptionDataCollection', '<xsl:call-template name="Exception">'],
  },
  {
    caption: 'Complex Data Rule',
    parentNode: 'Rules',
    typeName: 'Origam.Schema.RuleModel.ComplexDataRule',
    expectedLines: ['<RuleExceptionDataCollection', '<xsl:call-template name="Exception">'],
  },
];

async function createNewItem(page: Page, parentNode: string, typeName: string) {
  await page.getByTestId('tree-toggle-Business Logic').click();
  await page.getByTestId(`tree-node-${parentNode}`).click({ button: 'right' });
  await page.getByTestId('tree-menu-new').getByText('New').click();
  await page.getByTestId(`tree-menu-new-${typeName}`).click();
}

test.describe('Template of a new XSLT item (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  for (const { caption, parentNode, typeName, expectedLines } of cases) {
    test(`new ${caption} starts with the template`, async ({ page }) => {
      await page.goto('/');
      await createNewItem(page, parentNode, typeName);

      const codeArea = page.getByTestId('code-editor').first().locator('.view-lines');
      await expect(codeArea).toContainText(FIRST_LINE, { timeout: 30_000 });
      for (const line of expectedLines) {
        await expect(codeArea).toContainText(line);
      }
    });
  }
});
