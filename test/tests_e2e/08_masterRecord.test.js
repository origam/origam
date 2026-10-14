const { test, expect } = require("@playwright/test");
const { openMenuItem, login, waitForRowCountData, switchLanguageTo,
  beforeEachTest, waitForRowSelected } = require('./testTools');
const {widgetsMenuItemId,topMenuHeader, allDataTypesLazyDataViewId,
  sectionsMenuItemId, masterDetailLazyLoadedMenuItemId, allDataTypesLazyMenuItemsId
} = require("./modelIds");
const { clearScreenConfiguration, restoreWidgetSectionTestMasterForRefreshTest,
  restoreAllDataTypesTable} = require("./dbTools");

let page;

test.beforeAll(async() => {
  await clearScreenConfiguration();
  await restoreWidgetSectionTestMasterForRefreshTest();
});

test.beforeEach(async ({ page: testPage }) => {
  page = testPage;
  await beforeEachTest(page);
});




const allDataTypes_dataViewId = "dataView_e67865b0-ce91-413c-bed7-1da486399633";
const master_dataViewId = "dataView_775fa5ea-fa75-40a7-8c39-7828f7cdf508";
const details_dataViewId = "dataView_b11ffa85-7507-475c-af50-ef08fd56072c";
const subDetails_dataViewId = "dataView_3430ae94-8642-4cf8-9e68-9df36dcf571c";


const text1InputId = "editor_cb584956-8f34-4d95-852e-eff4680a2673";

async function cancelRowChange() {
  await page.locator("#cancelRecordChangeButton").click();
}

async function checkDataViewContainsSingleRow(page, dataViewId, tabName) {
  await page.locator(`div[title="${tabName}"]`).click();
  await waitForRowCountData(page, dataViewId, 1);
}

test.describe("Html client", () => {
  test("Should ask user whether to save changes after changing row", async () => {
    await restoreAllDataTypesTable();
    await switchLanguageTo({locale: "cs-CZ", page: page});
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        sectionsMenuItemId,
        allDataTypesLazyMenuItemsId,
      ]);

    await waitForRowCountData(page, allDataTypes_dataViewId,2099);
    await page.locator(`#${allDataTypesLazyDataViewId} .formPerspectiveButton`).click();
    await page.locator(`#${allDataTypesLazyDataViewId} .nextRowButton`).click();
    await waitForRowSelected(page,allDataTypesLazyDataViewId, 2);

    const textInput = page.locator(`#${text1InputId}`);
    await textInput.focus();
    await expect(textInput).toBeFocused();
    await textInput.pressSequentially("test");
    await page.locator(`#${allDataTypesLazyDataViewId} .tablePerspectiveButton`).click();

    // Click inside the table
    await page.locator(`#${allDataTypesLazyDataViewId} [class*='_cellAreaContainer']`)
      .click({ position: { x: 50, y: 90 } });
    await cancelRowChange();

    // Click add row button
    await page.locator(`#${allDataTypesLazyDataViewId} .addRow`).click();
    await cancelRowChange();

    // Click copy row button
    await page.locator(`#${allDataTypesLazyDataViewId} .copyRow`).click();
    await cancelRowChange();

    // Click first row button
    await page.locator(`#${allDataTypesLazyDataViewId} .firstRowButton`).click();
    await cancelRowChange();

    // Click previous row button
    await page.locator(`#${allDataTypesLazyDataViewId} .previousRowButton`).click();
    await cancelRowChange();

    // Click next row button
    await page.locator(`#${allDataTypesLazyDataViewId} .nextRowButton`).click();
    await cancelRowChange();

    // Click last row button
    await page.locator(`#${allDataTypesLazyDataViewId} .lastRowButton`).click();
    await cancelRowChange();
  });
  test("Should reload all data after refresh is pressed", async () => {
    await restoreAllDataTypesTable();
    await switchLanguageTo({locale: "cs-CZ", page: page});
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        sectionsMenuItemId,
        masterDetailLazyLoadedMenuItemId,
      ]);

    await waitForRowCountData(page, master_dataViewId, 2);

    await page.locator("#refreshButton").click();

    await waitForRowCountData(page, master_dataViewId, 2);

    await checkDataViewContainsSingleRow(page, details_dataViewId, "Details");
    await checkDataViewContainsSingleRow(page, subDetails_dataViewId, "Details-Subdetail");
    });
});
