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

interface SnapLine {
  kind: string;
  orientation: string;
  position: number;
  start: number;
  length: number;
}

function value(control: ApiControl, name: string): number {
  return propertyValue(control, name) as number;
}

async function snapLines(surface: Locator, kind: string): Promise<SnapLine[]> {
  return surface.locator(`[data-test-id="snap-line"][data-kind="${kind}"]`).evaluateAll(lines =>
    lines.map(line => {
      const rect = line.getBoundingClientRect();
      const isVertical = line.getAttribute('data-orientation') === 'vertical';
      return {
        kind: line.getAttribute('data-kind')!,
        orientation: line.getAttribute('data-orientation')!,
        position: Math.round(isVertical ? rect.left : rect.top),
        start: Math.round(isVertical ? rect.top : rect.left),
        length: Math.round(isVertical ? rect.height : rect.width),
      };
    }),
  );
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

  function rightHandle(control: ApiControl): Locator {
    return editor.component(control).locator('[class*="__resizeHandle__"][class*="__right__"]');
  }

  test('a widget dragged near the left edge of a neighbour snaps to it', async ({ page }) => {
    const [anchor, moved] = await dropTwoTextBoxes(40);
    await editor.setProperty('Width', String(value(anchor, 'Width') + 30));
    const anchorBox = (await editor.component(anchor).boundingBox())!;

    const start = await grab(page, editor.component(moved));
    await page.mouse.move(start.x - 37, start.y, { steps: 5 });
    await expect
      .poll(() => snapLines(editor.surface, 'alignment'))
      .toEqual([
        {
          kind: 'alignment',
          orientation: 'vertical',
          position: Math.round(anchorBox.x),
          start: Math.round(anchorBox.y),
          length: value(moved, 'Top') + value(moved, 'Height') - value(anchor, 'Top'),
        },
      ]);

    const root = await release(page);
    await expect(editor.surface.getByTestId('snap-line')).toHaveCount(0);
    const result = findControl(root, moved.id)!;
    expect(value(result, 'Left')).toBe(value(anchor, 'Left'));
    expect(value(result, 'Top')).toBe(value(moved, 'Top'));
  });

  test('a widget dragged level with a neighbour snaps to its center', async ({ page }) => {
    const [anchor, moved] = await dropTwoTextBoxes(150);
    const anchorBox = (await editor.component(anchor).boundingBox())!;
    const gap = value(moved, 'Top') - value(anchor, 'Top');

    const start = await grab(page, editor.component(moved));
    await page.mouse.move(start.x, start.y - gap + 4, { steps: 5 });
    await expect
      .poll(() => snapLines(editor.surface, 'alignment'))
      .toEqual([
        {
          kind: 'alignment',
          orientation: 'horizontal',
          position: Math.round(anchorBox.y + value(anchor, 'Height') / 2),
          start: Math.round(anchorBox.x),
          length: 150 + value(moved, 'Width'),
        },
      ]);

    const result = findControl(await release(page), moved.id)!;
    expect(value(result, 'Top')).toBe(value(anchor, 'Top'));
    expect(value(result, 'Left')).toBe(value(anchor, 'Left') + 150);
  });

  test('a widget dragged just below a neighbour snaps to touch it', async ({ page }) => {
    const anchor = await editor.dropWidget('AsTextBox', 'Text1');
    const moved = await editor.dropWidget('AsTextBox', 'Text2');
    const anchorBox = (await editor.component(anchor).boundingBox())!;
    const anchorBottom = value(anchor, 'Top') + value(anchor, 'Height');
    const gap = value(moved, 'Top') - anchorBottom;

    const start = await grab(page, editor.component(moved));
    await page.mouse.move(start.x, start.y - gap + 3, { steps: 5 });
    await expect
      .poll(() => snapLines(editor.surface, 'alignment'))
      .toContainEqual({
        kind: 'alignment',
        orientation: 'horizontal',
        position: Math.round(anchorBox.y + value(anchor, 'Height')),
        start: Math.round(anchorBox.x),
        length: value(anchor, 'Width'),
      });

    const result = findControl(await release(page), moved.id)!;
    expect(value(result, 'Top')).toBe(anchorBottom);
    expect(value(result, 'Left')).toBe(value(anchor, 'Left'));
  });

  test('a widget dragged beyond the snap distance stays where it was dropped', async ({ page }) => {
    const [anchor, moved] = await dropTwoTextBoxes(40);

    const start = await grab(page, editor.component(moved));
    await page.mouse.move(start.x - 35, start.y, { steps: 5 });
    await waitForCenter(editor.component(moved), start.x - 35, start.y);
    await expect(editor.surface.getByTestId('snap-line')).toHaveCount(0);

    const result = findControl(await release(page), moved.id)!;
    expect(value(result, 'Left')).toBe(value(anchor, 'Left') + 5);
  });

  test('holding Alt drags a widget without snapping', async ({ page }) => {
    const [anchor, moved] = await dropTwoTextBoxes(40);

    const start = await grab(page, editor.component(moved));
    await page.keyboard.down('Alt');
    await page.mouse.move(start.x - 37, start.y, { steps: 5 });
    await waitForCenter(editor.component(moved), start.x - 37, start.y);
    await expect(editor.surface.getByTestId('snap-line')).toHaveCount(0);

    const root = await release(page);
    await page.keyboard.up('Alt');
    expect(value(findControl(root, moved.id)!, 'Left')).toBe(value(anchor, 'Left') + 3);
  });

  test('the third widget in a column snaps to the spacing of the first two', async ({ page }) => {
    const first = await editor.dropWidget('AsTextBox', 'Text1');
    const second = await editor.dropWidget('AsTextBox', 'Text2');
    const moved = await editor.dropWidget('AsTextBox', 'Integer1');
    const height = value(first, 'Height');
    const spacing = 10;
    const secondTop = value(first, 'Top') + height + spacing;
    await editor.select(second);
    await editor.setProperty('Top', String(secondTop));
    const firstBox = (await editor.component(first).boundingBox())!;
    const targetTop = secondTop + height + spacing;

    const start = await grab(page, editor.component(moved));
    await page.mouse.move(start.x, start.y - value(moved, 'Top') + targetTop + 2, { steps: 5 });
    const guidePosition = Math.round(firstBox.x + value(first, 'Width'));
    await expect
      .poll(() => snapLines(editor.surface, 'spacing'))
      .toEqual([
        {
          kind: 'spacing',
          orientation: 'vertical',
          position: guidePosition,
          start: Math.round(firstBox.y + height),
          length: spacing,
        },
        {
          kind: 'spacing',
          orientation: 'vertical',
          position: guidePosition,
          start: Math.round(firstBox.y + 2 * height + spacing),
          length: spacing,
        },
      ]);

    const result = findControl(await release(page), moved.id)!;
    expect(value(result, 'Top')).toBe(targetTop);
  });

  test('a widget dragged between two neighbours snaps to the middle', async ({ page }) => {
    const first = await editor.dropWidget('AsTextBox', 'Text1');
    const second = await editor.dropWidget('AsTextBox', 'Text2');
    const moved = await editor.dropWidget('AsTextBox', 'Integer1');
    const width = value(first, 'Width');
    const spacing = 30;
    await editor.select(second);
    await editor.setProperty('Top', String(value(first, 'Top')));
    await editor.setProperty('Left', String(value(first, 'Left') + 2 * (width + spacing)));
    const firstBox = (await editor.component(first).boundingBox())!;
    const targetLeft = value(first, 'Left') + width + spacing;

    const start = await grab(page, editor.component(moved));
    await page.mouse.move(
      start.x - value(moved, 'Left') + targetLeft + 2,
      start.y - value(moved, 'Top') + value(first, 'Top') + 1,
      { steps: 5 },
    );
    const guidePosition = Math.round(firstBox.y + value(first, 'Height'));
    await expect
      .poll(() => snapLines(editor.surface, 'spacing'))
      .toEqual([
        {
          kind: 'spacing',
          orientation: 'horizontal',
          position: guidePosition,
          start: Math.round(firstBox.x + width),
          length: spacing,
        },
        {
          kind: 'spacing',
          orientation: 'horizontal',
          position: guidePosition,
          start: Math.round(firstBox.x + 2 * width + spacing),
          length: spacing,
        },
      ]);

    const result = findControl(await release(page), moved.id)!;
    expect(value(result, 'Left')).toBe(targetLeft);
    expect(value(result, 'Top')).toBe(value(first, 'Top'));
  });

  test('resizing a widget snaps its width to the width of a neighbour', async ({ page }) => {
    const anchor = await editor.dropWidget('AsTextBox', 'Text1');
    const resized = await editor.dropWidget('AsTextBox', 'Text2');
    const width = value(anchor, 'Width');
    await editor.select(resized);
    await editor.setProperty('Left', String(value(anchor, 'Left') + 20));
    await editor.setProperty('Width', String(width - 40));
    const anchorBox = (await editor.component(anchor).boundingBox())!;
    const resizedBox = (await editor.component(resized).boundingBox())!;

    const start = await grab(page, rightHandle(resized));
    await page.mouse.move(start.x + 37, start.y, { steps: 5 });
    await expect
      .poll(() => snapLines(editor.surface, 'size'))
      .toEqual([
        {
          kind: 'size',
          orientation: 'horizontal',
          position: Math.round(resizedBox.y + value(resized, 'Height') / 2),
          start: Math.round(resizedBox.x),
          length: width,
        },
        {
          kind: 'size',
          orientation: 'horizontal',
          position: Math.round(anchorBox.y + value(anchor, 'Height') / 2),
          start: Math.round(anchorBox.x),
          length: width,
        },
      ]);

    const result = findControl(await release(page), resized.id)!;
    expect(value(result, 'Width')).toBe(width);
    expect(value(result, 'Left')).toBe(value(anchor, 'Left') + 20);
  });

  test('resizing a widget snaps its right edge to the edge of a neighbour', async ({ page }) => {
    const [anchor, resized] = await dropTwoTextBoxes(30);
    const anchorBox = (await editor.component(anchor).boundingBox())!;

    const start = await grab(page, rightHandle(resized));
    await page.mouse.move(start.x - 27, start.y, { steps: 5 });
    await expect
      .poll(() => snapLines(editor.surface, 'alignment'))
      .toEqual([
        {
          kind: 'alignment',
          orientation: 'vertical',
          position: Math.round(anchorBox.x + value(anchor, 'Width')),
          start: Math.round(anchorBox.y),
          length: value(resized, 'Top') + value(resized, 'Height') - value(anchor, 'Top'),
        },
      ]);
    await expect(editor.surface.locator('[data-kind="size"]')).toHaveCount(0);

    const result = findControl(await release(page), resized.id)!;
    expect(value(result, 'Width')).toBe(value(anchor, 'Width') - 30);
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
});
