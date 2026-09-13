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

import { expect, test, type Locator, type Page, type Response } from '@playwright/test';
import fs from 'node:fs';
import { activatePackage } from '@support/activatePackage';
import { openConstants } from '@support/modelTree';
import { modelFilePath, resetBackend } from '@support/resetBackend';
import { setServerCulture } from '@support/setServerCulture';

const PACKAGE = 'Root';

const BOOLEAN_CONSTANT = 'InitialUserCreated';
const BOOLEAN_FILE = 'Root/DataConstant/InitialUserCreated.origam';
const STRING_CONSTANT = 'DefaultMailWorkQueueName';
const STRING_FILE = 'Root/DataConstant/DefaultMailWorkQueueName.origam';
// Its lookup fills the drop down only with a reachable database, the editor has
// to open either way.
const LOOKUP_CONSTANT = 'DimensionType_Sales';
const LOOKUP_GROUPS = ['Dimensions', 'DimensionType'];

const SERVER_CULTURES = ['cs-CZ', 'en-US'];
const DECIMAL_TYPES = ['Currency', 'Float'];

// The Yes/No labels are localized, so options are addressed by position.
const TRUE_OPTION = 0;
const FALSE_OPTION = 1;

test.describe('Data Constant editor (real backend)', () => {
  test.use({ actionTimeout: 10_000, navigationTimeout: 20_000 });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, PACKAGE);
  });

  // Regression: opening any constant returned 500.
  test('opens the editor of a Boolean constant', async ({ page }) => {
    const serverErrors = collectServerErrors(page);

    await openConstants(page);
    await openConstantEditor(page, BOOLEAN_CONSTANT);

    await expectNoServerErrors(page, serverErrors);
  });

  test('opens the editor of a constant bound to a lookup', async ({ page }) => {
    const serverErrors = collectServerErrors(page);

    await openConstants(page);
    for (const group of LOOKUP_GROUPS) {
      await page.getByTestId(`tree-toggle-${group}`).click();
    }
    await openConstantEditor(page, LOOKUP_CONSTANT);

    await expectNoServerErrors(page, serverErrors);
  });

  test('offers a drop down for a Boolean value', async ({ page }) => {
    await openConstants(page);
    await openConstantEditor(page, BOOLEAN_CONSTANT);
    await expect(valueInput(page)).toHaveCount(0);

    const input = valueSelect(page).locator('input');
    await input.click();

    await expect(dropDownOptions(page)).toHaveCount(2);
    await expect(input).toHaveValue(await optionText(page, TRUE_OPTION));
  });

  // The drop down works with texts, the model file has to get the typed value.
  test('stores a Boolean value as true or false', async ({ page }) => {
    await openConstants(page);
    await openConstantEditor(page, BOOLEAN_CONSTANT);

    await pickOption(page, valueSelect(page), FALSE_OPTION);
    await save(page);

    await expectModelFile(BOOLEAN_FILE, content => content.includes('dc:value="false"'));
  });

  test('offers a text box for a value with no drop down', async ({ page }) => {
    await openConstants(page);
    await openConstantEditor(page, STRING_CONSTANT);

    await expect(valueInput(page)).toHaveValue('MAIL');
    await expect(valueSelect(page)).toHaveCount(0);
  });

  test('stores an edited text value', async ({ page }) => {
    await openConstants(page);
    await openConstantEditor(page, STRING_CONSTANT);

    await fillProperty(page, 'Value', 'E2EQUEUE');
    await save(page);

    await expectModelFile(STRING_FILE, content => content.includes('dc:value="E2EQUEUE"'));
  });

  test('stores a cleared value', async ({ page }) => {
    await openConstants(page);
    await openConstantEditor(page, STRING_CONSTANT);

    await fillProperty(page, 'Value', '');
    await save(page);

    await expectModelFile(STRING_FILE, content => !content.includes('MAIL'));
  });

  test('switches the value editor when the data type changes', async ({ page }) => {
    await openConstants(page);
    await openConstantEditor(page, STRING_CONSTANT);
    await expect(valueInput(page)).toBeVisible();

    await pickNamedOption(page, page.getByTestId('property-select-DataType'), 'Boolean');

    await expect(valueSelect(page)).toBeVisible();
    await expect(valueInput(page)).toHaveCount(0);
  });

  test('reports a duplicate constant name', async ({ page }) => {
    const serverErrors = collectServerErrors(page);

    await openConstants(page);
    await openConstantEditor(page, STRING_CONSTANT);
    await fillProperty(page, 'Name', BOOLEAN_CONSTANT);

    await expect(page.getByTestId('property-label-Name')).toHaveAttribute(
      'title',
      new RegExp(`already uses the name '${BOOLEAN_CONSTANT}'`),
    );
    await expectNoServerErrors(page, serverErrors);
  });

  // The rule used to throw for every constant, unique names included.
  test('accepts a unique constant name', async ({ page }) => {
    await openConstants(page);
    await openConstantEditor(page, STRING_CONSTANT);
    await fillProperty(page, 'Name', 'E2EUniqueConstant');

    await expect
      .poll(() => page.getByTestId('property-label-Name').getAttribute('title'))
      .toBeNull();
  });

  for (const culture of SERVER_CULTURES) {
    test.describe(`on a ${culture} server`, () => {
      test.beforeEach(async ({ request }) => {
        await setServerCulture(request, culture);
      });

      test.afterEach(async ({ request }) => {
        await setServerCulture(request, null);
      });

      for (const dataType of DECIMAL_TYPES) {
        test(`stores a ${dataType} value typed with a decimal point`, async ({ page }) => {
          await openStringConstantAs(page, dataType);

          await fillProperty(page, 'Value', '1234.5');
          await save(page);

          await expectModelFile(STRING_FILE, content => content.includes('dc:value="1234.5"'));
        });

        test(`rejects a ${dataType} value typed with a decimal comma`, async ({ page }) => {
          await openStringConstantAs(page, dataType);

          await fillRejectedProperty(page, 'Value', '12,5');
          await save(page);

          await expectModelFile(STRING_FILE, content => !content.includes('dc:value="125"'));
        });
      }

      test('rejects an Integer value that is not a whole number', async ({ page }) => {
        await openStringConstantAs(page, 'Integer');

        await fillRejectedProperty(page, 'Value', '12,5');
      });

      test('stores a Date value typed as an ISO date', async ({ page }) => {
        await openStringConstantAs(page, 'Date');

        await fillProperty(page, 'Value', '2026-02-01');
        await save(page);

        await expectModelFile(STRING_FILE, content =>
          content.includes('dc:value="2026-02-01T00:00:00"'),
        );
      });

      test('rejects a Date value typed in a local format', async ({ page }) => {
        await openStringConstantAs(page, 'Date');

        await fillRejectedProperty(page, 'Value', '01.02.2026');
        await save(page);

        await expectModelFile(STRING_FILE, content => !content.includes('2026-01-02'));
      });
    });
  }
});

function valueSelect(page: Page): Locator {
  return page.getByTestId('property-select-Value');
}

function valueInput(page: Page): Locator {
  return page.getByTestId('property-input-Value');
}

// A 500 leaves the editor half rendered, which would only show up as a missing
// element later on.
function collectServerErrors(page: Page): string[] {
  const errors: string[] = [];
  page.on('response', response => {
    if (response.status() >= 500) {
      errors.push(`${response.status()} ${response.request().method()} ${response.url()}`);
    }
  });
  return errors;
}

// The editor keeps loading after the awaited element shows up, so the requests
// still in flight have to land before the collected errors mean anything.
async function expectNoServerErrors(page: Page, serverErrors: string[]): Promise<void> {
  await page.waitForLoadState('networkidle');
  expect(serverErrors).toEqual([]);
}

async function openConstantEditor(page: Page, nodeText: string): Promise<void> {
  await page.getByTestId(`tree-node-${nodeText}`).dblclick();
  await expect(page.getByTestId(`tab-${nodeText}`)).toBeVisible();
  // The property grid is what used to fail, so wait for a property of the item.
  await expect(page.getByTestId('property-input-Name')).toHaveValue(nodeText);
}

async function openStringConstantAs(page: Page, dataType: string): Promise<void> {
  await openConstants(page);
  await openConstantEditor(page, STRING_CONSTANT);
  await pickNamedOption(page, page.getByTestId('property-select-DataType'), dataType);
  await expect(valueInput(page)).toBeVisible();
}

async function fillProperty(page: Page, propertyName: string, value: string): Promise<void> {
  await awaitPropertyUpdate(page, () =>
    page.getByTestId(`property-input-${propertyName}`).fill(value),
  );
}

async function fillRejectedProperty(
  page: Page,
  propertyName: string,
  value: string,
): Promise<void> {
  const response = await propertyUpdateResponse(page, () =>
    page.getByTestId(`property-input-${propertyName}`).fill(value),
  );
  expect(response.status(), await response.text()).toBe(420);
  await page.getByRole('button', { name: 'Ok', exact: true }).click();
}

// The drop down renders into a portal on the body, virtual list spacers carry
// no text.
function dropDownOptions(page: Page): Locator {
  return page.locator('body > ul > li').filter({ hasText: /\S/ });
}

async function optionText(page: Page, index: number): Promise<string> {
  return (await dropDownOptions(page).nth(index).innerText()).trim();
}

async function pickOption(page: Page, select: Locator, index: number): Promise<void> {
  await select.locator('input').click();
  await awaitPropertyUpdate(page, () => dropDownOptions(page).nth(index).click());
}

async function pickNamedOption(page: Page, select: Locator, text: string): Promise<void> {
  await select.locator('input').click();
  await awaitPropertyUpdate(page, () =>
    dropDownOptions(page)
      .filter({ hasText: new RegExp(`^\\s*${text}\\s*$`) })
      .first()
      .click(),
  );
}

async function awaitPropertyUpdate(page: Page, action: () => Promise<void>): Promise<void> {
  const response = await propertyUpdateResponse(page, action);
  expect(response.ok(), await response.text()).toBeTruthy();
}

async function propertyUpdateResponse(page: Page, action: () => Promise<void>): Promise<Response> {
  const pendingResponse = page.waitForResponse(
    response => response.url().includes('/PropertyEditor/Update'),
    { timeout: 10_000 },
  );
  await action();
  return pendingResponse;
}

async function save(page: Page): Promise<void> {
  await page.getByTestId('save-button').click();
  await expect(page.getByTestId('save-button-disabled')).toBeVisible();
}

async function expectModelFile(
  relativePath: string,
  matches: (content: string) => boolean,
): Promise<void> {
  await expect
    .poll(() => matches(fs.readFileSync(modelFilePath(relativePath), 'utf8')), {
      message: relativePath,
      timeout: 5_000,
    })
    .toBe(true);
}
