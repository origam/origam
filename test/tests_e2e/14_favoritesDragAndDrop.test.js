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
const {topMenuHeader, widgetsMenuItemId, allDataTypesMenuId,
  allDataTypesLazyMenuItemsId} = require("./modelIds");
const { clearScreenConfiguration } = require("./dbTools");

let page;

test.beforeAll(async() => {
  await clearScreenConfiguration();
});

test.beforeEach(async ({ page: testPage }) => {
  page = testPage;
  await beforeEachTest(page);
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
  const header = page.locator(headerSelector);
  await header.hover();
  await page.locator(`${headerSelector} i.fa-edit`)
    .evaluate(element => element.click());
}

async function addToFavorites(page, menuItemId) {
  const menuItem = page.locator("div[class*='favoritesMenuItem']", {
    has: page.locator(`#${menuItemId}`),
  });
  const addButton = menuItem.locator("div[class*='addToFavoritesIconContainer']");
  const dialog = page.locator(dialogSelector);
  await expect(async () => {
    if (!await dialog.isVisible()) {
      await addButton.click();
    }
    await expect(dialog).toBeVisible({ timeout: 2_000 });
  }).toPass({ timeout: 20_000 });
  // The ok button of the folder dialog is focused when the dialog opens.
  await page.keyboard.press("Enter");
  await expect(dialog).toBeHidden();
}

// The favorites section and the menu section cannot be open at the same time
// unless the folder is pinned.
async function openFavoritesSection(page) {
  if (await page.locator(favoritesListSelector).count()) {
    return;
  }
  await page.locator(`${favoritesHeaderSelector} a`).click();
  await expect(page.locator(favoritesListSelector)).toBeVisible();
}

async function removeFromFavorites(page, menuItemId) {
  const favorite = page.locator(draggableSelector(menuItemId));
  if (await favorite.count() === 0) {
    return;
  }
  const editButton = page.locator(`${favoritesHeaderSelector} i.fa-edit`);
  if (!(await editButton.getAttribute("class")).includes("headerIconActive")) {
    await flipEditing(page, favoritesHeaderSelector);
  }
  const deleteIcon = favorite.locator("svg").first();
  await expect(deleteIcon).toBeVisible();
  await deleteIcon.click();
  await expect(favorite).toBeHidden();
}

async function getFavoritePosition(page, menuItemId) {
  const ids = await page.locator(`${favoritesListSelector} [data-rbd-draggable-id]`)
    .evaluateAll(elements => elements.map(element => element.getAttribute("data-rbd-draggable-id")));
  return ids.indexOf(draggableIdOf(menuItemId));
}

async function dragOnto(page, sourceSelector, targetSelector) {
  const source = page.locator(sourceSelector);
  const target = page.locator(targetSelector);
  const sourceBox = await source.boundingBox();
  const targetBox = await target.boundingBox();
  const startX = sourceBox.x + sourceBox.width / 2;
  const startY = sourceBox.y + sourceBox.height / 2;

  await page.mouse.move(startX, startY);
  await page.mouse.down();
  await waitForAnimationFrame(page);
  // react-beautiful-dnd ignores movements below its sloppy click threshold.
  await page.mouse.move(startX, startY - 10);
  await waitForAnimationFrame(page);
  await page.mouse.move(
    targetBox.x + targetBox.width / 2,
    targetBox.y + targetBox.height / 2 - 5,
    {steps: 20});
  await waitForAnimationFrame(page);
  await page.mouse.up();
}

async function waitForAnimationFrame(page) {
  await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => resolve())));
}

test.describe("Html client", () => {
  test("Should reorder favorites by dragging", async () => {
    await switchLanguageTo({locale: "cs-CZ", page: page});
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId
      ]);

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

    await expect.poll(async () => {
      const firstPositionAfter = await getFavoritePosition(page, allDataTypesMenuId);
      const secondPositionAfter = await getFavoritePosition(page, allDataTypesLazyMenuItemsId);
      return secondPositionAfter < firstPositionAfter;
    }).toBe(true);

    await removeFromFavorites(page, allDataTypesMenuId);
    await removeFromFavorites(page, allDataTypesLazyMenuItemsId);
  });
});
