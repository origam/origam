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
import fs from 'node:fs';
import path from 'node:path';
import { activatePackage } from '@support/activatePackage';
import { closeAiPanel } from '@support/aiPanel';
import {
  createTemporaryDirectory,
  removeTemporaryDirectory,
  startArchitectServer,
  type ArchitectServer,
} from '@support/architectServer';
import { collectServerErrors, expectNoServerErrors } from '@support/propertyEditor';
import { modelDirectory } from '@support/resetBackend';

const OLD_MODEL = path.join(__dirname, 'old-model');
const IMAGES_FOLDER = 'Root/Graphics';
const IMAGES = ['Home.png', 'Key.png', 'Report.png'];
const IMAGE_FILES = IMAGES.map(image => `${IMAGES_FOLDER}/${image}.origam`);
const STORED_PICTURE = `${IMAGES_FOLDER}/Home.png.origam___graphicsDataByte___0858cbb7-ff2f-4cc4-8727-57daa399012a.png`;
const IMAGE_ICON = '/Icons/icon_image.svg';
const ICON_SIZE = 16;

type ModelFiles = Map<string, Buffer>;

function readModelFiles(directory: string): ModelFiles {
  return new Map(
    fs
      .readdirSync(directory, { recursive: true, withFileTypes: true })
      .filter(entry => entry.isFile())
      .map(entry => {
        const filePath = path.join(entry.parentPath, entry.name);
        const relativePath = path.relative(directory, filePath).split(path.sep).join('/');
        return [relativePath, fs.readFileSync(filePath)];
      }),
  );
}

function changedFiles(before: ModelFiles, after: ModelFiles): string[] {
  return [...before.keys()]
    .filter(file => after.has(file) && !after.get(file)!.equals(before.get(file)!))
    .sort();
}

function filesMissingIn(model: ModelFiles, reference: ModelFiles): string[] {
  return [...reference.keys()].filter(file => !model.has(file)).sort();
}

function text(content: Buffer | undefined): string {
  return (content ?? Buffer.alloc(0)).toString('utf8').replaceAll('\r\n', '\n');
}

test.describe('Images saved by an older version (real backend)', () => {
  let modelCopy: string;
  let currentModel: ModelFiles;
  let oldModel: ModelFiles;
  let server: ArchitectServer;

  test.beforeAll(async () => {
    test.setTimeout(300_000);
    modelCopy = createTemporaryDirectory('origam-model-');
    fs.cpSync(modelDirectory, modelCopy, { recursive: true });
    currentModel = readModelFiles(modelCopy);
    fs.cpSync(OLD_MODEL, modelCopy, { recursive: true });
    oldModel = readModelFiles(modelCopy);
    server = await startArchitectServer(modelCopy);
  });

  test.afterAll(async () => {
    await server?.stop();
    removeTemporaryDirectory(modelCopy);
  });

  test('opening the model upgrades them and deletes their stored pictures', () => {
    const upgradedModel = readModelFiles(modelCopy);

    expect(changedFiles(oldModel, upgradedModel)).toEqual(IMAGE_FILES);
    expect(filesMissingIn(upgradedModel, oldModel)).toEqual([STORED_PICTURE]);
    expect(filesMissingIn(oldModel, upgradedModel)).toEqual([]);
    for (const imageFile of IMAGE_FILES) {
      expect(text(upgradedModel.get(imageFile))).toBe(text(currentModel.get(imageFile)));
    }
  });

  test('upgraded images show the icon of their type and open', async ({ page }) => {
    const serverErrors = collectServerErrors(page);
    await server.routePage(page);
    await activatePackage(server.request, 'Root Menu');

    await page.goto('/');
    await closeAiPanel(page);
    await page.getByTestId('tree-toggle-User Interface').click();
    await page.getByTestId('tree-toggle-Images').click();
    for (const image of IMAGES) {
      const icon = page.getByTestId(`tree-node-${image}`).locator('img');
      await expect(icon).toHaveAttribute('src', IMAGE_ICON);
      await expect
        .poll(() => icon.evaluate((element: HTMLImageElement) => element.naturalWidth))
        .toBe(ICON_SIZE);
    }

    await page.getByTestId('tree-node-Home.png').dblclick();
    await expect(page.getByTestId('tab-Home.png')).toBeVisible();
    await expect(page.getByTestId('property-input-Name')).toHaveValue('Home.png');
    await expectNoServerErrors(page, serverErrors);
  });
});
