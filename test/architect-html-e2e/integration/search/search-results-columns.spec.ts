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
import { closeAiPanel } from '@support/aiPanel';
import { resetBackend } from '@support/resetBackend';

const QUERY = 'IActive';
const MIN_COLUMN_WIDTH = 48;

async function searchResults(page: Page): Promise<Locator> {
  await page.goto('/');
  await closeAiPanel(page);
  await page.getByTestId('topbar-search-input').fill(QUERY);
  const table = page.getByTestId('schema-item-results-table').filter({ visible: true });
  await expect(table.locator('tbody tr').nth(1)).toBeVisible();
  return table;
}

function header(table: Locator, label: string): Locator {
  return table.getByRole('columnheader', { name: label, exact: true });
}

async function rowsOf(table: Locator, labels: string[]): Promise<string[][]> {
  const columnIndexes = await Promise.all(
    labels.map(label =>
      header(table, label).evaluate(cell => (cell as HTMLTableCellElement).cellIndex),
    ),
  );
  return table
    .locator('tbody tr')
    .evaluateAll(
      (rows, indexes) =>
        rows.map(row => indexes.map(index => (row as HTMLTableRowElement).cells[index].innerText)),
      columnIndexes,
    );
}

async function columnTexts(table: Locator, label: string): Promise<string[]> {
  return (await rowsOf(table, [label])).map(([text]) => text);
}

async function sortedInPage(page: Page, texts: string[]): Promise<string[]> {
  return page.evaluate(
    values => [...values].sort(new Intl.Collator(undefined, { numeric: true }).compare),
    texts,
  );
}

async function widthOf(locator: Locator): Promise<number> {
  return locator.evaluate(element => element.getBoundingClientRect().width);
}

async function dragBorder(page: Page, headerCell: Locator, targetX: (startX: number) => number) {
  const border = (await headerCell.getByRole('separator').boundingBox())!;
  const startX = border.x + border.width / 2;
  const middleY = border.y + border.height / 2;
  await page.mouse.move(startX, middleY);
  await page.mouse.down();
  await page.mouse.move(targetX(startX), middleY, { steps: 5 });
  await page.mouse.up();
}

async function horizontalOverflowAround(element: Locator): Promise<number> {
  return element.evaluate(start => {
    let overflow = 0;
    for (let ancestor = start.parentElement; ancestor; ancestor = ancestor.parentElement) {
      overflow = Math.max(overflow, ancestor.scrollWidth - ancestor.clientWidth);
    }
    return overflow;
  });
}

test.describe('Search result columns (real backend)', () => {
  test.use({ viewport: { width: 1600, height: 900 } });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('a header click sorts by its column and a second click reverses it', async ({ page }) => {
    const table = await searchResults(page);
    const foundInHeader = header(table, 'Found in');
    const typeHeader = header(table, 'Type');

    await expect(foundInHeader).toHaveAttribute('aria-sort', 'ascending');
    const foundIn = await columnTexts(table, 'Found in');
    expect(foundIn).toEqual(await sortedInPage(page, foundIn));

    await typeHeader.getByRole('button').click();
    await expect(typeHeader).toHaveAttribute('aria-sort', 'ascending');
    await expect(foundInHeader).not.toHaveAttribute('aria-sort');
    const types = await columnTexts(table, 'Type');
    expect(new Set(types).size).toBeGreaterThan(1);
    expect(types).toEqual(await sortedInPage(page, types));

    await typeHeader.getByRole('button').click();
    await expect(typeHeader).toHaveAttribute('aria-sort', 'descending');
    expect(await columnTexts(table, 'Type')).toEqual((await sortedInPage(page, types)).reverse());
  });

  test('Ctrl+click adds a column to the sort', async ({ page }) => {
    const table = await searchResults(page);
    const typeHeader = header(table, 'Type');
    const foundInHeader = header(table, 'Found in');

    await typeHeader.getByRole('button').click();
    await foundInHeader.getByRole('button').click({ modifiers: ['ControlOrMeta'] });
    await foundInHeader.getByRole('button').click({ modifiers: ['ControlOrMeta'] });

    await expect(typeHeader).toHaveAttribute('aria-sort', 'ascending');
    await expect(typeHeader).toContainText('1');
    await expect(foundInHeader).toContainText('2');
    const rows = await rowsOf(table, ['Type', 'Found in']);
    const expectedRows = await page.evaluate(pairs => {
      const collator = new Intl.Collator(undefined, { numeric: true });
      return [...pairs].sort(
        ([leftType, leftFoundIn], [rightType, rightFoundIn]) =>
          collator.compare(leftType, rightType) || collator.compare(rightFoundIn, leftFoundIn),
      );
    }, rows);
    expect(rows).toEqual(expectedRows);

    const rootTypeHeader = header(table, 'Root type');
    await rootTypeHeader.getByRole('button').click();
    await expect(rootTypeHeader).toHaveAttribute('aria-sort', 'ascending');
    await expect(typeHeader).not.toHaveAttribute('aria-sort');
    await expect(typeHeader).not.toContainText('1');
    await expect(foundInHeader).not.toContainText('2');
  });

  test('dragging a header border resizes only that column', async ({ page }) => {
    const table = await searchResults(page);
    const foundInHeader = header(table, 'Found in');
    const rootTypeHeader = header(table, 'Root type');
    const typeHeader = header(table, 'Type');
    const foundInWidth = await widthOf(foundInHeader);
    const rootTypeWidth = await widthOf(rootTypeHeader);

    await typeHeader.getByRole('separator').click();
    await expect(typeHeader).not.toHaveAttribute('aria-sort');

    await dragBorder(page, foundInHeader, startX => startX + 150);
    expect(Math.abs((await widthOf(foundInHeader)) - (foundInWidth + 150))).toBeLessThanOrEqual(1);
    expect(Math.abs((await widthOf(rootTypeHeader)) - rootTypeWidth)).toBeLessThanOrEqual(1);

    await dragBorder(page, foundInHeader, () => 1);
    expect(Math.round(await widthOf(foundInHeader))).toBe(MIN_COLUMN_WIDTH);
    expect(Math.abs((await widthOf(rootTypeHeader)) - rootTypeWidth)).toBeLessThanOrEqual(1);
  });

  test('a table wider than the tab scrolls inside its wrapper', async ({ page }) => {
    const table = await searchResults(page);
    const wrapper = table.locator('xpath=..');

    await dragBorder(page, header(table, 'Found in'), () => page.viewportSize()!.width - 1);

    expect(
      await wrapper.evaluate(element => element.scrollWidth - element.clientWidth),
    ).toBeGreaterThan(0);
    expect(await horizontalOverflowAround(wrapper)).toBe(0);
    await expect(header(table, 'Found in')).toBeVisible();
  });
});
