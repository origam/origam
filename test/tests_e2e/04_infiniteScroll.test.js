const { test, expect } = require("@playwright/test");
const { openMenuItem, login, waitForRowCountData,
  beforeEachTest
} = require('./testTools');
const {widgetsMenuItemId, allDataTypesLazyMenuItemsId, topMenuHeader} = require("./modelIds");
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

async function scrollToLastRow(page) {
  const scrollbar = page.locator(`#${dataViewId} .horiz-scrollbar`);
  const modelInstanceId = dataViewId.slice("dataView_".length);
  let tableData;

  await expect.poll(async () => {
    await scrollbar.evaluate(element => element.scrollTop = element.scrollHeight);
    tableData = await page.evaluate(key => window.tableDebugMonitor?.[key], modelInstanceId);
    return tableData?.data?.at(-1)?.Text1;
  }, { timeout: 60_000 }).toBe("txt2109");

  return tableData;
}

test.describe("Html client", () => {
  test("Should scroll to the end", async () => {
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        allDataTypesLazyMenuItemsId
      ]);
    await waitForRowCountData(page, dataViewId,2099);
    await expect(page.locator(`#${dataViewId} .horiz-scrollbar`)).toBeVisible();

    const tableData = await scrollToLastRow(page);
    const rows = tableData.data;
    const lastRow = rows[rows.length - 1];
    expect(lastRow["Text1"]).toBe("txt2109");

  });
});
