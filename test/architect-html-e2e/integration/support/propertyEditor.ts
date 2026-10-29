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

import { expect, type Page, type Response } from '@playwright/test';
import { readModelFile } from '@support/resetBackend';

// A 500 leaves the editor half rendered and would show up only as a missing element.
export function collectServerErrors(page: Page): string[] {
  const errors: string[] = [];
  page.on('response', response => {
    if (response.status() >= 500) {
      errors.push(`${response.status()} ${response.request().method()} ${response.url()}`);
    }
  });
  return errors;
}

// Requests still in flight after the awaited element have to land first.
export async function expectNoServerErrors(page: Page, serverErrors: string[]): Promise<void> {
  await page.waitForLoadState('networkidle');
  expect(serverErrors).toEqual([]);
}

export async function fillProperty(page: Page, propertyName: string, value: string): Promise<void> {
  await awaitPropertyUpdate(page, () => fillAndLeave(page, propertyName, value));
}

export async function fillRejectedProperty(
  page: Page,
  propertyName: string,
  value: string,
): Promise<void> {
  const response = await propertyUpdateResponse(page, () =>
    fillAndLeave(page, propertyName, value),
  );
  expect(response.status(), await response.text()).toBe(420);
  await page.getByRole('button', { name: 'Ok', exact: true }).click();
}

export async function awaitPropertyUpdate(page: Page, action: () => Promise<void>): Promise<void> {
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

export async function save(page: Page): Promise<void> {
  await page.getByTestId('save-button').click();
  await expect(page.getByTestId('save-button-disabled')).toBeVisible();
}

export async function expectModelFileContent(
  relativePath: string,
  matches: (content: string) => boolean,
): Promise<void> {
  await expect
    .poll(() => matches(readModelFile(relativePath)), {
      message: relativePath,
      timeout: 5_000,
    })
    .toBe(true);
}

// A typed value is sent only once the input is left.
async function fillAndLeave(page: Page, propertyName: string, value: string): Promise<void> {
  const input = page.getByTestId(`property-input-${propertyName}`);
  await input.fill(value);
  await input.press('Tab');
}
