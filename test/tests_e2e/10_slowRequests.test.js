const { test, expect } = require("@playwright/test");
const { openMenuItem, login, waitForRowCount, beforeEachTest } = require('./testTools');
const {widgetsMenuItemId, allDataTypesMenuId, topMenuHeader, allDataTypesDataViewId
} = require("./modelIds");
const {restoreAllDataTypesTable, clearScreenConfiguration} = require("./dbTools");

let page;

test.beforeAll(async() => {
  await restoreAllDataTypesTable();
  await clearScreenConfiguration();
});

test.beforeEach(async ({ page: testPage }) => {
  page = testPage;
  await beforeEachTest(page);
});



const dataViewId = "dataView_e67865b0-ce91-413c-bed7-1da486399633";
const text1InputId ="editor_cb584956-8f34-4d95-852e-eff4680a2673";

async function refreshAndThrowChangesAway() {
  await page.locator("#refreshButton").click();
  await page.locator("#dontSaveButton").click();
}

test.describe("Html client", () => {
  test("Should fill characters typed before the new row in grid is rendered", async () => {
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        allDataTypesMenuId
      ]);

    await waitForRowCount(page, dataViewId,30);

    let markCreateObjectReady;
    let releaseCreateObjectResponse;
    const createObjectReady = new Promise(resolve => {
      markCreateObjectReady = resolve;
    });
    const createObjectResponseCanContinue = new Promise(resolve => {
      releaseCreateObjectResponse = resolve;
    });
    await page.route("**/internalApi/UIService/CreateObject", async route => {
      const response = await route.fetch();
      markCreateObjectReady();
      await createObjectResponseCanContinue;
      await route.fulfill({ response });
    });

    await page.evaluate(() => document.activeElement?.blur());
    await page.locator(`#${allDataTypesDataViewId} .addRow`)
      .evaluate(element => element.click());
    await createObjectReady;
    await expect.poll(async () => page.evaluate(() => document.activeElement?.tagName))
      .not.toMatch(/^(INPUT|TEXTAREA)$/);

    // Hold the completed response so these keys arrive immediately before the first-column
    // input is rendered and focused, independent of server response time.
    await page.keyboard.type("abc");
    releaseCreateObjectResponse();

    const input = page.locator(`#${text1InputId}`);
    await expect(input).toBeFocused();

    await page.keyboard.press("d");
    await page.keyboard.press("e");
    await page.keyboard.press("f");

    await expect(input).toHaveValue(/abcdef/);

    await refreshAndThrowChangesAway();
  });
});

