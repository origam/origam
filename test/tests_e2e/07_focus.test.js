const { test, expect } = require("@playwright/test");
const { openMenuItem, login, waitForRowCountData, switchLanguageTo,
  beforeEachTest } = require('./testTools');
const {widgetsMenuItemId,topMenuHeader, allDataTypesLazyDataViewId,
  masterDataViewId, sectionsMenuItemId, masterDetailLazyLoadedMenuItemId, allDataTypesLazyMenuItemsId
} = require("./modelIds");
const { clearScreenConfiguration,
  restoreAllDataTypesTable} = require("./dbTools");
const {openFilters, setFilter} = require("./filterTestTools");

let page;

test.beforeAll(async() => {
  await clearScreenConfiguration();
});

test.beforeEach(async ({ page: testPage }) => {
  page = testPage;
  await beforeEachTest(page);
});




const dataViewId = "dataView_e67865b0-ce91-413c-bed7-1da486399633";
const text1PropertyId = "cb584956-8f34-4d95-852e-eff4680a2673";
const date1PropertyId ="c8e93248-81c0-4274-9ff1-1b7688944877";


test.describe("Html client", () => {
  test("Should keep focus in next input after tab", async () => {
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
    const firstColumnEditorId = "editor_b2adeca9-7f20-410d-bbe5-fb78e29614c2";
    const secondColumnEditorId = "editor_8b796084-3347-4ad0-8380-00a373176bb0";
    await page.locator(`#${masterDataViewId} .addRow`).click();
    const firstColumnEditor = page.locator(`#${firstColumnEditorId}`);
    await expect(firstColumnEditor).toBeFocused();
    await firstColumnEditor.pressSequentially("test");
    await firstColumnEditor.press("Tab");
    await expect(page.locator(`#${secondColumnEditorId}`)).toBeFocused();
  });
  test("Should keep focus in the filter field after change", async () => {
    await login(page);

    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        allDataTypesLazyMenuItemsId
      ]);

    await waitForRowCountData(page, dataViewId,2099);
    await openFilters({
      page: page,
      dataViewId: dataViewId,
      aPropertyId: date1PropertyId
    });

    const text1PropertyFilterInputId = "input_" + text1PropertyId;

    const filterInput = page.locator(`#${text1PropertyFilterInputId}`);
    await filterInput.focus();
    await setFilter({
      page: page,
      propertyId: text1PropertyId ,
      comboOptionText: "contains",
      value: "2"
    });

    await waitForRowCountData(page, dataViewId,651);
    await expect(filterInput).toBeFocused();
  });
  test("Should set focus to editor in the first column after adding a new row", async () => {
    await login(page);

    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        allDataTypesLazyMenuItemsId
      ]);
    await waitForRowCountData(page, dataViewId,2099);
    await page.locator(`#${allDataTypesLazyDataViewId} .addRow`).click();
    const text1PropertyEditorId = "editor_" + text1PropertyId;
    await expect(page.locator(`#${text1PropertyEditorId}`)).toBeFocused();
  });
});
