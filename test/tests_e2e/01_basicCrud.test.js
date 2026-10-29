const { test, expect } = require("@playwright/test");
const { getRowCountData, openMenuItem, login, waitForRowCountData, waitForRowSelected,
  beforeEachTest
} = require('./testTools');
const {widgetsMenuItemId, sectionsMenuItemId, masterDetailMenuItemId,
  topMenuHeader, masterDataViewId, detailDataViewId, detailEditorId, detailTabHandelId} = require("./modelIds");
const { restoreWidgetSectionTestMaster, clearScreenConfiguration} = require("./dbTools");

let page;

test.beforeAll(async() => {
  await restoreWidgetSectionTestMaster();
  await clearScreenConfiguration();
});

test.beforeEach(async ({ page: testPage }) => {
  page = testPage;
  await beforeEachTest(page);
});

async function addRowToMaster(firstColumnValue, secondColumnValue) {
  const firstColumnEditorId = "editor_b2adeca9-7f20-410d-bbe5-fb78e29614c2";
  const secondColumnEditorId = "editor_8b796084-3347-4ad0-8380-00a373176bb0";
  const firstColumnEditor = page.locator(`#${firstColumnEditorId}`);
  const secondColumnEditor = page.locator(`#${secondColumnEditorId}`);
  await clickAddRowAndWaitForEditor(masterDataViewId, firstColumnEditor);
  await expect(firstColumnEditor).toBeFocused();
  await performAndWaitForUiResponse(
    async () => {
      await firstColumnEditor.pressSequentially(firstColumnValue);
      await firstColumnEditor.press("Tab");
    },
    "UpdateObject"
  );
  await expect(secondColumnEditor).toBeFocused();
  await performAndWaitForUiResponse(
    async () => {
      await secondColumnEditor.pressSequentially(secondColumnValue);
      await secondColumnEditor.evaluate(element => element.blur());
    },
    "UpdateObject"
  );
}

async function addRowToDetail(detailValue) {
  const detailEditor = page.locator(`#${detailEditorId}`);
  await clickAddRowAndWaitForEditor(detailDataViewId, detailEditor);
  await expect(detailEditor).toBeFocused();
  await performAndWaitForUiResponse(
    async () => {
      await detailEditor.pressSequentially(detailValue);
      await detailEditor.evaluate(element => element.blur());
    },
    "UpdateObject"
  );
}

async function clickAddRowAndWaitForEditor(dataViewId, editor) {
  const initialCount = Number((await getRowCountData(page, dataViewId)).rowCount);
  const addRowButton = page.locator(`#${dataViewId} .addRow`);
  await performAndWaitForUiResponse(() => addRowButton.click(), "CreateObject");

  await expect.poll(async () =>
    Number((await getRowCountData(page, dataViewId)).rowCount)
  , { timeout: 120_000 }).toBe(initialCount + 1);
  await expect(editor).toBeFocused();
}

async function performAndWaitForUiResponse(action, endpoint) {
  const responsePromise = page.waitForResponse(response =>
    response.request().method() === "POST" &&
    response.url().endsWith(`/internalApi/UIService/${endpoint}`)
  , { timeout: 120_000 });
  const [response] = await Promise.all([responsePromise, action()]);
  expect(response.ok()).toBe(true);
}

async function refreshAndThrowChangesAway() {
  await page.locator("#refreshButton").click();

  await page.locator("#dontSaveButton").click();

  await page.locator(`#${detailTabHandelId}`).click();
  const detailRowCountData = await waitForRowCountData(page, detailDataViewId, 0);
  expect(detailRowCountData.selectedRow).toBe("-");
  expect(detailRowCountData.rowCount).toBe("0");

  const masterRowCountData = await waitForRowCountData(page, masterDataViewId, 0);
  expect(masterRowCountData.selectedRow).toBe("-");
  expect(masterRowCountData.rowCount).toBe("0");
}

async function selectMasterRow(rowIndex) {
  const rowHeight = 30;
  await page
    .locator(`#${masterDataViewId} [class*='_cellAreaContainer']`)
    .click({ position: { x: 50, y: rowHeight / 2 + rowHeight * rowIndex } });
}

test.describe("Html client", () => {
  test("Should perform basic CRUD", async () => {
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        sectionsMenuItemId,
        masterDetailMenuItemId,
      ]);

    // Add three rows
    await addRowToMaster("str11", "str12");
    await addRowToMaster("str21", "str22");
    await addRowToMaster("str31", "str32");

    const rowCountData = await waitForRowCountData(page, masterDataViewId, 3);
    expect(rowCountData.selectedRow).toBe("3");
    expect(rowCountData.rowCount).toBe("3");

    // remove the second row
    const tableArea = page.locator(`#${masterDataViewId} [class*='_cellAreaContainer']`);
    await tableArea.click({ position: { x: 50, y: 45 } });
    const deleteRowButton = page.locator(`#${masterDataViewId} .deleteRow`);
    const rowCountDataBeforeDelete = await waitForRowSelected(page, masterDataViewId, 2);
    expect(rowCountDataBeforeDelete.selectedRow).toBe("2");
    expect(rowCountDataBeforeDelete.rowCount).toBe("3");
    await deleteRowButton.click();

    await page.locator("#yesButton").click();

    const rowCountDataAfterDelete = await waitForRowCountData(page, masterDataViewId, 2);
    expect(rowCountDataAfterDelete.selectedRow).toBe("2");
    expect(rowCountDataAfterDelete.rowCount).toBe("2");
    //duplicate first row
    await tableArea.click({ position: { x: 50, y: 15 } });

    const copyRowButton = page.locator(`#${masterDataViewId} .copyRow`);
    await copyRowButton.click();
    const rowCountDataAfterDuplication = await waitForRowCountData(page, masterDataViewId, 3);
    expect(rowCountDataAfterDuplication.selectedRow).toBe("3");
    expect(rowCountDataAfterDuplication.rowCount).toBe("3");
    await refreshAndThrowChangesAway();
  });
  test("Should perform basic master detail interaction", async () => {
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        sectionsMenuItemId,
        masterDetailMenuItemId,
      ]);
    // Add rows to master
    await addRowToMaster("str11", "str12");
    await addRowToMaster("str21", "str22");

    // Add a row to detail (second line in the master active)
    await page.locator(`#${detailTabHandelId}`).click();
    await page.locator(`#${detailDataViewId} .formPerspectiveButton`).click();

    await addRowToDetail("detail2");
    await selectMasterRow(0);
    let detailRowCountData = await waitForRowCountData(page, detailDataViewId, 0);
    expect(detailRowCountData.selectedRow).toBe("-");
    expect(detailRowCountData.rowCount).toBe("0");

    await selectMasterRow(1);
    detailRowCountData = await waitForRowCountData(page, detailDataViewId, 1);
    expect(detailRowCountData.selectedRow).toBe("1");
    expect(detailRowCountData.rowCount).toBe("1");

    await refreshAndThrowChangesAway();
  });
});
