const { sleep, openMenuItem, login, waitForRowCountData, switchToFormPerspective,
  inputByPressingKeys, switchLanguageTo, waitForFocus,
  switchToTablePerspective, catchRequests, beforeEachTest, afterEachTest, xPathContainsClass, waitForRowCount,
  waitForRowSelected
} = require('./testTools');
const {widgetsMenuItemId,topMenuHeader, allDataTypesLazyDataViewId,
  masterDataViewId, sectionsMenuItemId, masterDetailLazyLoadedMenuItemId, allDataTypesLazyMenuItemsId, detailDataViewId
} = require("./modelIds");
const { clearScreenConfiguration,
  restoreAllDataTypesTable} = require("./dbTools");
const expect = require("expect");
const {openFilters, setFilter} = require("./filterTestTools");
const {installMouseHelper} = require("./instalMouseHelper_");

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


const dataViewId = "dataView_e67865b0-ce91-413c-bed7-1da486399633";
const text1PropertyId = "cb584956-8f34-4d95-852e-eff4680a2673";
const date1PropertyId ="c8e93248-81c0-4274-9ff1-1b7688944877";

const text1InputId = "editor_cb584956-8f34-4d95-852e-eff4680a2673";

async function cancelRowChange() {
  const cancelButton = await page.waitForSelector(
    `#cancelRecordChangeButton`,
    {visible: true}
  );
  await cancelButton.click();
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

    await waitForRowCountData(page, dataViewId,2099);
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
});
