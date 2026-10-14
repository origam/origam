const { sleep, openMenuItem, login, waitForRowCountData, switchToFormPerspective,
  inputByPressingKeys, switchLanguageTo, waitForFocus,
  switchToTablePerspective, catchRequests, beforeEachTest, afterEachTest, xPathContainsClass
} = require('./testTools');
const {widgetsMenuItemId,topMenuHeader, allDataTypesLazyDataViewId,
  masterDataViewId, sectionsMenuItemId, masterDetailLazyLoadedMenuItemId, allDataTypesLazyMenuItemsId
} = require("./modelIds");
const { clearScreenConfiguration,
  restoreAllDataTypesTable} = require("./dbTools");
const expect = require("expect");
const {openFilters, setFilter} = require("./filterTestTools");

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


describe("Html client", () => {
  it("Should keep focus in next input after tab", async () => {
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
    let waitForRequests = catchRequests(page);
    await page.waitForXPath(
      `//div[@id='${masterDataViewId}']//div[${xPathContainsClass("addRow")}]`,
      { visible: true }
    );

    const firstColumnEditorId = "editor_b2adeca9-7f20-410d-bbe5-fb78e29614c2";
    const secondColumnEditorId = "editor_8b796084-3347-4ad0-8380-00a373176bb0";

    await sleep(500);
    await waitForRequests;
    await sleep(300);
    waitForRequests = catchRequests(page);
    await page.$eval(`#${masterDataViewId} .addRow`, elem => elem.click());

    await page.waitForFunction(`document.activeElement == document.getElementById("${firstColumnEditorId}")`);
    await page.focus(`#${firstColumnEditorId}`)
    await page.keyboard.type("test")
    await page.keyboard.press("Tab");
    await sleep(200);
    await waitForRequests;

    await page.waitForFunction(`document.activeElement == document.getElementById("${secondColumnEditorId}")`);
  });
  it("Should keep focus in the filter field after change", async () => {
    await login(page);

    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        allDataTypesLazyMenuItemsId
      ]);

    await waitForRowCountData(page, dataViewId,2099);

    await sleep(300);
    let waitForRequests = catchRequests(page);

    await openFilters({
      page: page,
      dataViewId: dataViewId,
      aPropertyId: date1PropertyId
    });

    await sleep(300);

    const text1PropertyFilterInputId = "input_" + text1PropertyId;

    await page.focus(`#${text1PropertyFilterInputId}`)
    await setFilter({
      page: page,
      propertyId: text1PropertyId ,
      comboOptionText: "contains",
      value: "2"
    });

    await waitForRowCountData(page, dataViewId,651);
    await waitForRequests;

    await sleep(200);
    await page.waitForFunction(`document.activeElement == document.getElementById("${text1PropertyFilterInputId}")`);
  });
  it("Should set focus to editor in the first column after adding a new row", async () => {
    await login(page);

    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        allDataTypesLazyMenuItemsId
      ]);
    let waitForRequests = catchRequests(page);
    await waitForRowCountData(page, dataViewId,2099);

    await page.waitForXPath(
      `//div[@id='${allDataTypesLazyDataViewId}']//div[${xPathContainsClass("addRow")}]`,
      { visible: true }
    );

    await sleep(500);
    await waitForRequests;
    await sleep(300);
    waitForRequests = catchRequests(page);
    await page.$eval(`#${allDataTypesLazyDataViewId} .addRow`, elem => elem.click());

    await waitForRequests;

    await sleep(200);
    const text1PropertyEditorId = "editor_" + text1PropertyId;
    await page.waitForFunction(`document.activeElement == document.getElementById("${text1PropertyEditorId}")`);
  });
});
