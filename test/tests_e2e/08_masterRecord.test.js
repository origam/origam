const { sleep, openMenuItem, login, waitForRowCountData, switchToFormPerspective,
  inputByPressingKeys, switchLanguageTo, waitForFocus,
  switchToTablePerspective, catchRequests, beforeEachTest, afterEachTest, xPathContainsClass, waitForRowCount,
  waitForRowSelected
} = require('./testTools');
const {widgetsMenuItemId,topMenuHeader, allDataTypesLazyDataViewId,
  masterDataViewId, sectionsMenuItemId, masterDetailLazyLoadedMenuItemId, allDataTypesLazyMenuItemsId, detailDataViewId
} = require("./modelIds");
const { clearScreenConfiguration, restoreWidgetSectionTestMasterForRefreshTest,
  restoreAllDataTypesTable} = require("./dbTools");
const expect = require("expect");
const {openFilters, setFilter} = require("./filterTestTools");
const {installMouseHelper} = require("./installMouseHelper_");

let browser;
let page;

beforeAll(async() => {
  await clearScreenConfiguration();
  await restoreWidgetSectionTestMasterForRefreshTest();
});

beforeEach(async () => {
  [browser, page] = await beforeEachTest()
});

afterEach(async () => {
  await afterEachTest(browser);
  browser = undefined;
});


const allDataTypes_dataViewId = "dataView_e67865b0-ce91-413c-bed7-1da486399633";
const master_dataViewId = "dataView_775fa5ea-fa75-40a7-8c39-7828f7cdf508";
const details_dataViewId = "dataView_b11ffa85-7507-475c-af50-ef08fd56072c";
const subDetails_dataViewId = "dataView_3430ae94-8642-4cf8-9e68-9df36dcf571c";


const text1InputId = "editor_cb584956-8f34-4d95-852e-eff4680a2673";

async function cancelRowChange() {
  const cancelButton = await page.waitForSelector(
    `#cancelRecordChangeButton`,
    {visible: true}
  );
  await cancelButton.click();
}

async function checkDataViewContainsSingleRow(page, dataViewId, tabName) {
  const [button] = await page.$x(`//div[@title="${tabName}"]`);
  if (!button) {
    throw Error(`tab ${tabName} not found.`);
  }
  await button.click();
  await sleep(300);
  await waitForRowCountData(page, dataViewId, 1);
}

describe("Html client", () => {
  it("Should ask user whether to save changes after changing row", async () => {
    await restoreAllDataTypesTable();
    await switchLanguageTo({locale: "cs-CZ", page: page});
    await login(page);
    await installMouseHelper(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        sectionsMenuItemId,
        allDataTypesLazyMenuItemsId,
      ]);

    await waitForRowCountData(page, allDataTypes_dataViewId,2099);
    await sleep(500);
    const formPerspectiveButton = await page.$(`#${allDataTypesLazyDataViewId} .formPerspectiveButton`);
    await formPerspectiveButton.click();

    await sleep(500);
    const nextButton = await page.$(`#${allDataTypesLazyDataViewId} .nextRowButton`);
    await nextButton.click();
    await waitForRowSelected(page,allDataTypesLazyDataViewId, 2);

    await page.focus(`#${text1InputId}`)
    await page.waitForFunction(`document.activeElement == document.getElementById("${text1InputId}")`);
    await page.keyboard.type("test")

    await sleep(500);
    const tablePerspectiveButton = await page.$(`#${allDataTypesLazyDataViewId} .tablePerspectiveButton `);
    await tablePerspectiveButton.click();

    const tableArea = await page.$(`#${allDataTypesLazyDataViewId}  [class*='_cellAreaContainer']`);
    const box = await tableArea.boundingBox();
    // Click inside the table
    await sleep(500);
    await page.mouse.click(
      box.x + 50,
      box.y + 90
    );
    await cancelRowChange();

    // Click add row button
    await sleep(500);
    const addRowButton = await page.$(`#${allDataTypesLazyDataViewId} .addRow`);
    await addRowButton.click();
    await cancelRowChange();

    // Click copy row button
    await sleep(500);
    const copyRowButton = await page.$(`#${allDataTypesLazyDataViewId} .copyRow`);
    await copyRowButton.click();
    await cancelRowChange();

    // Click first row button
    await sleep(500);
    const firstRowButton = await page.$(`#${allDataTypesLazyDataViewId} .firstRowButton`);
    await firstRowButton.click();
    await cancelRowChange();

    // Click previous row button
    await sleep(500);
    const previousRowButton = await page.$(`#${allDataTypesLazyDataViewId} .previousRowButton`);
    await previousRowButton.click();
    await cancelRowChange();

    // Click next row button
    await sleep(500);
    const nextRowButton = await page.$(`#${allDataTypesLazyDataViewId} .nextRowButton`);
    await nextRowButton.click();
    await cancelRowChange();

    // Click last row button
    await sleep(500);
    const lastRowButton = await page.$(`#${allDataTypesLazyDataViewId} .lastRowButton`);
    await lastRowButton.click();
    await cancelRowChange();
  });
  it("Should reload all data after refresh is pressed", async () => {
    await restoreAllDataTypesTable();
    await switchLanguageTo({locale: "cs-CZ", page: page});
    await login(page);
    await installMouseHelper(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        sectionsMenuItemId,
        masterDetailLazyLoadedMenuItemId,
      ]);

    await waitForRowCountData(page, master_dataViewId, 2);
    await sleep(500);

    const [button] = await page.$x(`//div[@id="refreshButton"]`);
    if (!button) {
      throw Error(`#refreshButton not found.`);
    }
    await button.click();

    await waitForRowCountData(page, master_dataViewId, 2);
    await sleep(500);

    await checkDataViewContainsSingleRow(page, details_dataViewId, "Details");
    await checkDataViewContainsSingleRow(page, subDetails_dataViewId, "Details-Subdetail");
    });
});