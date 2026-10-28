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

const { test, expect } = require("@playwright/test");
const { openMenuItem, login, switchLanguageTo, beforeEachTest } = require('./testTools');
const {topMenuHeader, settingsMenuFolderId, generalMenuFolderId,
  systemColorsMenuItemId} = require("./modelIds");
const { clearScreenConfiguration } = require("./dbTools");

let page;

test.beforeAll(async() => {
  await clearScreenConfiguration();
});

test.beforeEach(async ({ page: testPage }) => {
  page = testPage;
  await beforeEachTest(page);
});



// The color editor renders no ids, only css module classes.
const colorTriggerSelector = "[class*='_colorDiv']";
const colorRectSelector = "[class*='_colorRect']";
const sketchPickerSelector = ".sketch-picker";
const presetColor = "#D0021B";

test.describe("Html client", () => {
  test("Should open the color picker and apply a picked color", async () => {
    await switchLanguageTo({locale: "cs-CZ", page: page});
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        settingsMenuFolderId,
        generalMenuFolderId,
        systemColorsMenuItemId
      ]);

    await page.locator(".formPerspectiveButton").click();
    await expect(page.locator(colorTriggerSelector)).toBeVisible();

    await page.locator(colorTriggerSelector).click();
    await expect(page.locator(sketchPickerSelector)).toBeVisible();

    // react-color draws the checkerboard from Checkboard.defaultProps, which React 19
    // ignores on function components. Without the react-color patch this part throws.
    const hasCheckerboard = await page.locator(`${sketchPickerSelector} div`).evaluateAll(
      elements => elements.some(element =>
        getComputedStyle(element).backgroundImage.startsWith('url("data:image/png'))
    );
    expect(hasCheckerboard).toBe(true);

    await page.locator(`${sketchPickerSelector} [title='${presetColor}']`).click();

    await expect(page.locator(colorRectSelector)).toHaveCSS("background-color", "rgb(208, 2, 27)");

    // Enter applies the picked color and closes the picker, Escape would revert it.
    await page.keyboard.press("Enter");

    await expect(page.locator(sketchPickerSelector)).toHaveCount(0);
    await expect(page.locator(colorRectSelector)).toHaveCSS("background-color", "rgb(208, 2, 27)");
  });
});
