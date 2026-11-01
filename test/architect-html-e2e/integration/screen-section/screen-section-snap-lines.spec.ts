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
import { resetBackend } from '@support/resetBackend';
import { type ApiControl, findControl, propertyValue, SectionEditor } from '@support/sectionEditor';

type Orientation = 'vertical' | 'horizontal';

function value(control: ApiControl, name: string): number {
  return propertyValue(control, name) as number;
}

async function snapLinePositions(surface: Locator, orientation: Orientation): Promise<number[]> {
  return surface
    .locator(`[data-test-id="snap-line"][data-orientation="${orientation}"]`)
    .evaluateAll(
      (lines, isVertical) =>
        lines.map(line => {
          const rect = line.getBoundingClientRect();
          return Math.round(isVertical ? rect.left : rect.top);
        }),
      orientation === 'vertical',
    );
}

async function marginLineSpans(surface: Locator): Promise<{ start: number; length: number }[]> {
  return surface.locator('[data-test-id="snap-line"][data-kind="margin"]').evaluateAll(lines =>
    lines.map(line => {
      const rect = line.getBoundingClientRect();
      const isVertical = line.getAttribute('data-orientation') === 'vertical';
      return {
        start: Math.round(isVertical ? rect.top : rect.left),
        length: Math.round(isVertical ? rect.height : rect.width),
      };
    }),
  );
}

async function baselinePositions(surface: Locator): Promise<number[]> {
  return surface
    .locator('[data-test-id="snap-line"][data-kind="baseline"]')
    .evaluateAll(lines => lines.map(line => Math.round(line.getBoundingClientRect().top)));
}

async function textTop(element: Locator): Promise<number> {
  return element.evaluate(node => {
    const range = document.createRange();
    range.selectNodeContents(node);
    return Math.round(range.getBoundingClientRect().top);
  });
}

async function grab(page: Page, target: Locator): Promise<{ x: number; y: number }> {
  const box = (await target.boundingBox())!;
  const center = { x: box.x + box.width / 2, y: box.y + box.height / 2 };
  await page.mouse.move(center.x, center.y);
  await page.mouse.down();
  return center;
}

async function waitForCenter(target: Locator, x: number, y: number) {
  await expect
    .poll(async () => {
      const box = (await target.boundingBox())!;
      return [Math.round(box.x + box.width / 2), Math.round(box.y + box.height / 2)];
    })
    .toEqual([Math.round(x), Math.round(y)]);
}

async function release(page: Page): Promise<ApiControl> {
  const updateSent = page.waitForRequest(request =>
    request.url().includes('/SectionEditor/Update'),
  );
  await page.mouse.up();
  const response = (await (await updateSent).response())!;
  expect(response.ok(), await response.text()).toBeTruthy();
  return (await response.json()).data.rootControl as ApiControl;
}

test.describe('Screen section snap lines (real backend)', () => {
  let editor: SectionEditor;

  test.beforeEach(async ({ page, request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
    editor = new SectionEditor(page, request);
    await editor.openNew('AllDataTypes');
  });

  async function dropTwoTextBoxes(secondLeftOffset: number): Promise<[ApiControl, ApiControl]> {
    const first = await editor.dropWidget('AsTextBox', 'Text1');
    const second = await editor.dropWidget('AsTextBox', 'Text2');
    await editor.select(second);
    await editor.setProperty('Left', String(value(first, 'Left') + secondLeftOffset));
    return [first, second];
  }

  test('a widget dragged near the left edge of a neighbour snaps to it', async ({ page }) => {
    const [anchor, moved] = await dropTwoTextBoxes(40);
    const anchorBox = (await editor.component(anchor).boundingBox())!;

    const start = await grab(page, editor.component(moved));
    await page.mouse.move(start.x - 35, start.y, { steps: 5 });
    await expect
      .poll(() => snapLinePositions(editor.surface, 'vertical'))
      .toContain(Math.round(anchorBox.x));

    const root = await release(page);
    await expect(editor.surface.getByTestId('snap-line')).toHaveCount(0);
    const result = findControl(root, moved.id)!;
    expect(value(result, 'Left')).toBe(value(anchor, 'Left'));
    expect(value(result, 'Top')).toBe(value(moved, 'Top'));
  });

  test('a widget dragged near the top edge of a neighbour snaps to it', async ({ page }) => {
    const [anchor, moved] = await dropTwoTextBoxes(150);
    const anchorBox = (await editor.component(anchor).boundingBox())!;
    const gap = value(moved, 'Top') - value(anchor, 'Top');

    const start = await grab(page, editor.component(moved));
    await page.mouse.move(start.x, start.y - gap + 4, { steps: 5 });
    await expect
      .poll(() => snapLinePositions(editor.surface, 'horizontal'))
      .toContain(Math.round(anchorBox.y));

    const result = findControl(await release(page), moved.id)!;
    expect(value(result, 'Top')).toBe(value(anchor, 'Top'));
    expect(value(result, 'Left')).toBe(value(anchor, 'Left') + 150);
  });

  test('a widget dragged beyond the snap distance stays where it was dropped', async ({ page }) => {
    const [anchor, moved] = await dropTwoTextBoxes(40);

    const start = await grab(page, editor.component(moved));
    await page.mouse.move(start.x - 28, start.y, { steps: 5 });
    await waitForCenter(editor.component(moved), start.x - 28, start.y);
    await expect(editor.surface.getByTestId('snap-line')).toHaveCount(0);

    const result = findControl(await release(page), moved.id)!;
    expect(value(result, 'Left')).toBe(value(anchor, 'Left') + 12);
  });

  test('holding Alt drags a widget without snapping', async ({ page }) => {
    const [anchor, moved] = await dropTwoTextBoxes(40);

    const start = await grab(page, editor.component(moved));
    await page.keyboard.down('Alt');
    await page.mouse.move(start.x - 35, start.y, { steps: 5 });
    await waitForCenter(editor.component(moved), start.x - 35, start.y);
    await expect(editor.surface.getByTestId('snap-line')).toHaveCount(0);

    const root = await release(page);
    await page.keyboard.up('Alt');
    expect(value(findControl(root, moved.id)!, 'Left')).toBe(value(anchor, 'Left') + 5);
  });

  test('resizing a widget snaps its right edge to a neighbour', async ({ page }) => {
    const anchor = await editor.dropWidget('AsTextBox', 'Text1');
    const resized = await editor.dropWidget('AsTextBox', 'Text2');
    expect(value(resized, 'Left')).toBe(value(anchor, 'Left'));
    await editor.select(resized);
    await editor.setProperty('Width', String(value(anchor, 'Width') - 40));
    const anchorBox = (await editor.component(anchor).boundingBox())!;

    const rightHandle = editor
      .component(resized)
      .locator('[class*="__resizeHandle__"][class*="__right__"]');
    const start = await grab(page, rightHandle);
    await page.mouse.move(start.x + 35, start.y, { steps: 5 });
    await expect
      .poll(() => snapLinePositions(editor.surface, 'vertical'))
      .toContain(Math.round(anchorBox.x + value(anchor, 'Width')));

    const result = findControl(await release(page), resized.id)!;
    expect(value(result, 'Width')).toBe(value(anchor, 'Width'));
  });

  test('a widget inside a group box does not snap to a widget outside it', async ({ page }) => {
    const groupBox = await editor.dropWidget('GroupBox');
    const inner = await editor.dropWidget('AsTextBox', 'Text1', groupBox);
    const surfaceBox = (await editor.surface.boundingBox())!;
    const groupBoxBox = (await editor.component(groupBox).boundingBox())!;
    const outer = await editor.dropWidget('AsTextBox', 'Text2', undefined, {
      x: groupBoxBox.x + groupBoxBox.width + 50 - surfaceBox.x,
      y: groupBoxBox.y + 110 - surfaceBox.y,
    });
    const innerBox = (await editor.component(inner).boundingBox())!;
    const outerBox = (await editor.component(outer).boundingBox())!;
    const dy = Math.round(outerBox.y - innerBox.y) + 4;

    const start = await grab(page, editor.component(inner));
    await page.mouse.move(start.x, start.y + dy, { steps: 5 });
    await waitForCenter(editor.component(inner), start.x, start.y + dy);
    await expect(editor.surface.getByTestId('snap-line')).toHaveCount(0);

    const result = findControl(await release(page), inner.id)!;
    expect(value(result, 'Top')).toBe(value(inner, 'Top') + dy);
  });

  test('a widget dragged near the spacing below a neighbour snaps to it', async ({ page }) => {
    const anchor = await editor.dropWidget('AsTextBox', 'Text1');
    const moved = await editor.dropWidget('AsTextBox', 'Text2');
    const anchorBox = (await editor.component(anchor).boundingBox())!;
    const anchorBottom = value(anchor, 'Top') + value(anchor, 'Height');
    const gap = value(moved, 'Top') - anchorBottom;

    const start = await grab(page, editor.component(moved));
    await page.mouse.move(start.x, start.y - gap + 10, { steps: 5 });
    await expect
      .poll(() => marginLineSpans(editor.surface))
      .toContainEqual({ start: Math.round(anchorBox.y + value(anchor, 'Height')), length: 6 });

    const result = findControl(await release(page), moved.id)!;
    expect(value(result, 'Top')).toBe(anchorBottom + 6);
  });

  test('a widget dragged near the spacing beside a neighbour snaps to it', async ({ page }) => {
    const [anchor, moved] = await dropTwoTextBoxes(140);
    const anchorBox = (await editor.component(anchor).boundingBox())!;
    const anchorWidth = value(anchor, 'Width');

    const start = await grab(page, editor.component(moved));
    await page.mouse.move(
      start.x + anchorWidth + 9 - 140,
      start.y - value(moved, 'Top') + value(anchor, 'Top'),
      { steps: 5 },
    );
    await expect
      .poll(() => marginLineSpans(editor.surface))
      .toContainEqual({ start: Math.round(anchorBox.x + anchorWidth), length: 6 });

    const result = findControl(await release(page), moved.id)!;
    expect(value(result, 'Left')).toBe(value(anchor, 'Left') + anchorWidth + 6);
    expect(value(result, 'Top')).toBe(value(anchor, 'Top'));
  });

  test('a label dragged near the baseline of a field caption snaps to it', async ({ page }) => {
    const field = await editor.dropWidget('AsTextBox', 'Text1', undefined, { x: 150, y: 120 });
    await editor.select(field);
    await editor.chooseProperty('CaptionPosition', 'Top');
    const label = await editor.dropWidget('Label');
    await editor.select(label);
    await editor.setProperty('Text', 'Hello');
    await editor.setProperty('Left', String(value(field, 'Left') + 120));
    const caption = editor.surface.getByText('Text 1', { exact: true });
    const labelText = editor.component(label).getByText('Hello', { exact: true });
    const captionBox = (await caption.boundingBox())!;
    const misalignment = 4;
    const dy = (await textTop(caption)) - (await textTop(labelText)) + misalignment;

    const start = await grab(page, editor.component(label));
    await page.mouse.move(start.x, start.y + dy, { steps: 5 });
    await expect
      .poll(async () =>
        (await baselinePositions(editor.surface)).some(
          position => position >= captionBox.y && position < captionBox.y + captionBox.height,
        ),
      )
      .toBe(true);

    await release(page);
    await expect.poll(() => textTop(labelText)).toBe(await textTop(caption));
  });
});
