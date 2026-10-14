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

const { sleep, openMenuItem, login, switchLanguageTo, clickAndWaitForSelector,
  beforeEachTest, afterEachTest
} = require('./testTools');
const {topMenuHeader, settingsMenuFolderId, generalMenuFolderId,
  systemColorsMenuItemId} = require("./modelIds");
const { clearScreenConfiguration } = require("./dbTools");
const expect = require("expect");

let browser;
let page;

beforeAll(async() => {
  await clearScreenConfiguration();
});

beforeEach(async () => {
  [browser, page] = await beforeEachTest()
});

afterEach(async () => {
  await afterEachTest(browser);
  browser = undefined;
});

// The color editor renders no ids, only css module classes.
const colorTriggerSelector = "[class*='_colorDiv']";
const colorRectSelector = "[class*='_colorRect']";
const sketchPickerSelector = ".sketch-picker";
const presetColor = "#D0021B";

describe("Html client", () => {
  it("Should open the color picker and apply a picked color", async () => {
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
    await sleep(1000);

    const formPerspectiveButton = await page.waitForSelector(
      ".formPerspectiveButton",
      {visible: true});
    await sleep(300);
    await clickAndWaitForSelector({
      page: page,
      clickable: formPerspectiveButton,
      selector: colorTriggerSelector
    });

    const colorTrigger = await page.waitForSelector(colorTriggerSelector, {visible: true});
    await clickAndWaitForSelector({
      page: page,
      clickable: colorTrigger,
      selector: sketchPickerSelector
    });

    // react-color draws the checkerboard from Checkboard.defaultProps, which React 19
    // ignores on function components. Without the react-color patch this part throws.
    const hasCheckerboard = await page.$$eval(
      `${sketchPickerSelector} div`,
      elements => elements.some(element =>
        getComputedStyle(element).backgroundImage.startsWith('url("data:image/png')));
    expect(hasCheckerboard).toBe(true);

    const presetSwatch = await page.waitForSelector(
      `${sketchPickerSelector} [title='${presetColor}']`,
      {visible: true});
    await presetSwatch.click();
    await sleep(500);

    const rectColor = await page.$eval(
      colorRectSelector,
      element => getComputedStyle(element).backgroundColor);
    expect(rectColor).toBe("rgb(208, 2, 27)");

    // Enter applies the picked color and closes the picker, Escape would revert it.
    await page.keyboard.press("Enter");
    await sleep(500);

    expect(await page.$(sketchPickerSelector)).toBeNull();
    const appliedColor = await page.$eval(
      colorRectSelector,
      element => getComputedStyle(element).backgroundColor);
    expect(appliedColor).toBe("rgb(208, 2, 27)");
  });
});
