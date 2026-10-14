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

const { sleep, openMenuItem, login, switchLanguageTo,
  beforeEachTest, afterEachTest
} = require('./testTools');
const {topMenuHeader, widgetsMenuItemId, allDataTypesMenuId,
  allDataTypesLazyMenuItemsId} = require("./modelIds");
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

const menuHeaderSelector = "[class*='_topMenuHeader']";
const favoritesHeaderSelector = "[class*='_favoritesFolderHeader']";
const favoritesListSelector = "[class*='_favoritesList']";
const dialogSelector = "[class*='_dialogContent']";

function draggableIdOf(menuItemId) {
  return "favorite_item_" + menuItemId.substring("menu_".length);
}

function draggableSelector(menuItemId) {
  return `[data-rbd-draggable-id='${draggableIdOf(menuItemId)}']`;
}

// The edit button is hidden until the section header is hovered. Enabling editing
// in one section disables it in all the others.
async function flipEditing(page, headerSelector) {
  const header = await page.waitForSelector(headerSelector, {visible: true});
  await header.hover();
  await sleep(300);
  const editButton = await page.waitForSelector(`${headerSelector} i.fa-edit`, {visible: true});
  await editButton.click();
  await sleep(300);
}

async function addToFavorites(page, menuItemId) {
  const [addButton] = await page.$x(
    `//div[contains(@class,'favoritesMenuItem')][.//*[@id='${menuItemId}']]` +
    `//div[contains(@class,'addToFavoritesIconContainer')]`);
  if (!addButton) {
    throw new Error(`Add to favorites button of ${menuItemId} was not found.`);
  }
  await addButton.click();
  await page.waitForSelector(dialogSelector, {visible: true});
  await sleep(300);
  // The ok button of the folder dialog is focused when the dialog opens.
  await page.keyboard.press("Enter");
  await page.waitForSelector(dialogSelector, {hidden: true});
  await sleep(300);
}

// The favorites section and the menu section cannot be open at the same time
// unless the folder is pinned.
async function openFavoritesSection(page) {
  if (await page.$(favoritesListSelector)) {
    return;
  }
  const header = await page.waitForSelector(`${favoritesHeaderSelector} a`, {visible: true});
  await header.click();
  await page.waitForSelector(favoritesListSelector, {visible: true});
  await sleep(300);
}

async function removeFromFavorites(page, menuItemId) {
  const deleteIcon = await page.$(`${draggableSelector(menuItemId)} [class*='_deleteIcon']`);
  if (!deleteIcon) {
    console.warn(`Delete icon of ${menuItemId} was not found, the favorite was left behind.`);
    return;
  }
  await deleteIcon.click();
  await sleep(500);
}

async function getFavoritePosition(page, menuItemId) {
  const ids = await page.$$eval(
    `${favoritesListSelector} [data-rbd-draggable-id]`,
    elements => elements.map(element => element.getAttribute("data-rbd-draggable-id")));
  return ids.indexOf(draggableIdOf(menuItemId));
}

async function dragOnto(page, sourceSelector, targetSelector) {
  const source = await page.waitForSelector(sourceSelector, {visible: true});
  const target = await page.waitForSelector(targetSelector, {visible: true});
  const sourceBox = await source.boundingBox();
  const targetBox = await target.boundingBox();
  const startX = sourceBox.x + sourceBox.width / 2;
  const startY = sourceBox.y + sourceBox.height / 2;

  await page.mouse.move(startX, startY);
  await page.mouse.down();
  await sleep(200);
  // react-beautiful-dnd ignores movements below its sloppy click threshold.
  await page.mouse.move(startX, startY - 10);
  await sleep(200);
  await page.mouse.move(
    targetBox.x + targetBox.width / 2,
    targetBox.y + targetBox.height / 2 - 5,
    {steps: 20});
  await sleep(500);
  await page.mouse.up();
  await sleep(1000);
}

describe("Html client", () => {
  it("Should reorder favorites by dragging", async () => {
    await switchLanguageTo({locale: "cs-CZ", page: page});
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId
      ]);
    await sleep(500);

    await flipEditing(page, menuHeaderSelector);
    await addToFavorites(page, allDataTypesMenuId);
    await addToFavorites(page, allDataTypesLazyMenuItemsId);

    await openFavoritesSection(page);
    await flipEditing(page, favoritesHeaderSelector);

    const firstPosition = await getFavoritePosition(page, allDataTypesMenuId);
    const secondPosition = await getFavoritePosition(page, allDataTypesLazyMenuItemsId);
    expect(firstPosition).toBeGreaterThanOrEqual(0);
    expect(firstPosition).toBeLessThan(secondPosition);

    await dragOnto(
      page,
      draggableSelector(allDataTypesLazyMenuItemsId),
      draggableSelector(allDataTypesMenuId));

    const firstPositionAfter = await getFavoritePosition(page, allDataTypesMenuId);
    const secondPositionAfter = await getFavoritePosition(page, allDataTypesLazyMenuItemsId);
    expect(secondPositionAfter).toBeLessThan(firstPositionAfter);

    await removeFromFavorites(page, allDataTypesMenuId);
    await removeFromFavorites(page, allDataTypesLazyMenuItemsId);
  });
});
