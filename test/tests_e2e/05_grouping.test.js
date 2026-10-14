const { test, expect } = require("@playwright/test");
const { openMenuItem, login, waitForRowCountData,
  waitForRowSelected, performAndWaitForTableRender, beforeEachTest
} = require('./testTools');
const {widgetsMenuItemId, allDataTypesMenuId, allDataTypesLazyMenuItemsId, topMenuHeader} = require("./modelIds");
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
const text1PropertyId = "cb584956-8f34-4d95-852e-eff4680a2673";
const integer1PropertyId = "3f3f6be7-6e87-48d7-9ac1-89ac30dc43ce";
const boolean1PropertyId ="d63fbdbb-3bbc-43c9-a9f2-a8585c42bbae";
const date1PropertyId ="c8e93248-81c0-4274-9ff1-1b7688944877";
const comboPropertyId ="14be2199-ad7f-43c3-83bf-a27c1fa66f7c";
const tagPropertyId ="3c685902-b55b-45cb-807c-01e8386bb313";

// Opens the column configuration dialog and returns its OK button. Clicking the menu
// item closes the dropdown, so a retry has to open the menu again instead of clicking
// the detached item.
async function openColumnConfiguration() {
  await page.locator(`#${dataViewId} .threeDotMenu`).click();
  await page.locator("#columnConfigItem").click();
  const okButton = page.locator("#columnConfigOk");
  await expect(okButton).toBeVisible();
  return okButton;
}

async function setGroupingCheckbox(propertyId, expectedIndex) {
  const checkbox = page.locator(`#group_by_${propertyId}`);
  const index = page.locator(`#group_index_${propertyId}`);

  await expect(async () => {
    if ((await index.innerText()) !== expectedIndex) {
      await checkbox.click();
    }
    await expect(index).toHaveText(expectedIndex, { timeout: 2_000 });
  }).toPass({ timeout: 20_000 });
}

async function setGrouping(expectedRowCount) {
  const columnConfigOk = await openColumnConfiguration();

  await setGroupingCheckbox(text1PropertyId, "1");
  await setGroupingCheckbox(integer1PropertyId, "2");
  await columnConfigOk.click();
  await waitForRowCountData(page, dataViewId, expectedRowCount);
}

async function clearGrouping() {
  const columnConfigOk = await openColumnConfiguration();

  await setGroupingCheckbox(text1PropertyId, "");
  await setGroupingCheckbox(integer1PropertyId, "");
  return performAndWaitForTableRender(page, dataViewId, () => columnConfigOk.click());
}

async function clickTableAndWaitForRender(tableArea, position) {
  await performAndWaitForTableRender(page, dataViewId, () => tableArea.click({ position }));
}

async function clickLazyGroupAndWaitForResponse(tableArea, position, endpoint) {
  await performAndWaitForTableRender(page, dataViewId, async () => {
    const responsePromise = page.waitForResponse(response =>
      response.request().method() === "POST" && response.url().endsWith(endpoint)
    );
    const [response] = await Promise.all([
      responsePromise,
      tableArea.click({ position }),
    ]);
    expect(response.ok()).toBe(true);
  });
}

test.describe("Html client", () => {
  test("Should perform grouping tests", async () => {
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        allDataTypesMenuId
      ]);

    await waitForRowCountData(page, dataViewId, 30);

    await setGrouping(30);

    const rowHeight = 25;
    const tableArea = page.locator(`#${dataViewId} [class*='_cellAreaContainer']`);

    await clickTableAndWaitForRender(tableArea, {
      x: rowHeight / 2,
      y: rowHeight / 2,
    });
    await clickTableAndWaitForRender(tableArea, {
      x: rowHeight + (rowHeight / 5),
      y: rowHeight * 2.5,
    });
    await tableArea.click({
      position: { x: rowHeight * 2, y: rowHeight * 3.5 },
    });

    await waitForRowCountData(page, dataViewId, "2 (30)");
    const rowData = await waitForRowSelected(page, dataViewId, 1);
    expect(rowData && rowData.rowCount).toBe( '2 (30)');

    const ungroupedTableData = await clearGrouping();
    expect(ungroupedTableData.data).toHaveLength(30);
  })
  test("Should perform grouping tests lazy loaded", async () => {
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        allDataTypesLazyMenuItemsId
      ]);

    await waitForRowCountData(page, dataViewId, 2099);

    await setGrouping(2099);

    const rowHeight = 25;
    const tableArea = page.locator(`#${dataViewId} [class*='_cellAreaContainer']`);

    await clickLazyGroupAndWaitForResponse(tableArea, {
      x: rowHeight / 2,
      y: rowHeight / 2,
    }, "/internalApi/UIService/GetGroups");
    await clickLazyGroupAndWaitForResponse(tableArea, {
      x: rowHeight + (rowHeight / 5),
      y: rowHeight * 2.5,
    }, "/internalApi/UIService/GetRows");
    await tableArea.click({
      position: { x: rowHeight * 2, y: rowHeight * 3.5 },
    });

    const rowData = await waitForRowSelected(page, dataViewId, 1);
    expect(rowData && rowData.rowCount).toBe( '2 (2099)');

    await clearGrouping()

    await waitForRowCountData(page, dataViewId, 2099);
  })
});
