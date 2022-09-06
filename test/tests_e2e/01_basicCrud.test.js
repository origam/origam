const puppeteer = require("puppeteer");
const { backEndUrl} = require('./additionalConfig');
const { sleep, xPathContainsClass, openMenuItem, login, getRowCountData, catchRequests, waitForRowCount} = require('./testTools');
const {widgetsMenuItemId, sectionsMenuItemId, masterDerailMenuItemId, topMenuHeader} = require("./modelIds");
const { restoreWidgetSectionTestMaster, clearScreenConfiguration} = require("./consoleTools");

let browser;
let page;

beforeAll(async() => {
  await restoreWidgetSectionTestMaster();
  await clearScreenConfiguration();
});

beforeEach(async () => {
  browser = await puppeteer.launch({
    ignoreHTTPSErrors: true,
    //devtools: true,
    headless: false,
    defaultViewport: {
      width: 1024,
      height: 768,
    },
    args: [
        "--disable-gpu",
        "--disable-dev-shm-usage",
        "--disable-setuid-sandbox",
        "--no-sandbox",
    ]
  });
  page = await browser.newPage();
  await page.goto(backEndUrl);
  await page.evaluate(() => {
    localStorage.setItem("debugCloseAllForms", "1");
  });
});

afterEach(async () => {
  let pages = await browser.pages();
  await Promise.all(pages.map(page =>page.close()));
  await sleep(200);
  if(browser) await browser.close();
  browser = undefined;
});

const masterDataViewId = "dataView_775fa5ea-fa75-40a7-8c39-7828f7cdf508";
const detailDataViewId = "dataView_b11ffa85-7507-475c-af50-ef08fd56072c";
const detailEditorId = "editor_89be97a4-86e8-4036-b57a-36155e3f2322";
const detailTabHandelId = "tabHandle_823ea459-bca5-476f-ab6f-9cb07769923e";


async function addRowToMaster(firstColumnValue, secondColumnValue) {
  const firstColumnEditorId = "editor_b2adeca9-7f20-410d-bbe5-fb78e29614c2";
  const secondColumnEditorId = "editor_8b796084-3347-4ad0-8380-00a373176bb0";

  await sleep(300);
  let waitForRequests = catchRequests(page);
  await page.$eval(`#${masterDataViewId} .addRow`, elem => elem.click());

  await page.waitForFunction(`document.activeElement == document.getElementById("${firstColumnEditorId}")`);
  await page.focus(`#${firstColumnEditorId}`)
  await page.keyboard.type(firstColumnValue)

  await sleep(200);
  await page.focus(`#${firstColumnEditorId}`)
  await page.keyboard.press("Tab");

  await page.waitForFunction(`document.activeElement == document.getElementById("${secondColumnEditorId}")`);

  await page.focus(`#${secondColumnEditorId}`)
  await page.keyboard.type(secondColumnValue)
  await waitForRequests;
  await sleep(100);
}

async function addRowToDetail(detailValue) {
  await sleep(200);
  await page.$eval(`#${detailDataViewId} .addRow`, elem => elem.click());
  await page.waitForFunction(`document.activeElement == document.getElementById("${detailEditorId}")`);
  await sleep(100);
  await page.focus(`#${detailEditorId}`)
  await page.keyboard.type(detailValue)
  await sleep(100);
}

async function refreshAndThrowChangesAway() {
  await page.$eval("#refreshButton", elem => elem.click());

  const dontSaveButton = await page.waitForXPath(
    `//button[@id='dontSaveButton']`,
    {visible: true}
  );
  await dontSaveButton.click();

  const detailTabHandle = await page.$(`#${detailTabHandelId}`);
  await detailTabHandle.click();

  await sleep(500);
  const detailRowCountData = await getRowCountData(page, detailDataViewId);
  expect(detailRowCountData.selectedRow).toBe("-");
  expect(detailRowCountData.rowCount).toBe("0");

  const masterRowCountData = await getRowCountData(page, masterDataViewId);
  expect(masterRowCountData.selectedRow).toBe("-");
  expect(masterRowCountData.rowCount).toBe("0");
}

async function selectMasterRow(rowIndex) {
  const rowHeight = 30;
  const tableArea = await page.$(`#${masterDataViewId}  [class*='Table_cellAreaContainer']`);
  const box = await tableArea.boundingBox();
  await page.mouse.click(
    box.x + 50,
    box.y + rowHeight / 2 + rowHeight * rowIndex
  );
}

describe("Html client", () => {
  it("Should perform basic CRUD", async () => {
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        sectionsMenuItemId,
        masterDerailMenuItemId,
      ]);

    // Add three rows
    await page.waitForXPath(
      `//div[@id='${masterDataViewId}']//div[${xPathContainsClass("addRow")}]`,
      { visible: true }
    );

    await addRowToMaster("str11", "str12");
    await addRowToMaster("str21", "str22");
    await addRowToMaster("str31", "str32");

    // await page.$eval("#saveButton", elem => elem .click())
    // await page.waitForSelector('#saveButton :not(.isRed)');

    await sleep(300);
    const rowCountData = await getRowCountData(page, masterDataViewId);
    expect(rowCountData.selectedRow).toBe("3");
    expect(rowCountData.rowCount).toBe("3");

    // const scroller = await page.$(`#${masterDataViewId}  [class*='Table_cellAreaContainer']`);
    // const imageAfterAdding = await getImage(page, scroller)
    // expect(imageAfterAdding).toMatchImageSnapshot();


    // remove the second row
    await sleep(500);
    const tableArea = await page.$(`#${masterDataViewId}  [class*='Table_cellAreaContainer']`);
    const box = await tableArea.boundingBox();
    await page.mouse.click(
      box.x + 50,
      box.y + 45
    );
    const deleteRowButton = await page.waitForXPath(
      `//div[@id='${masterDataViewId}']//div[${xPathContainsClass("deleteRow")}]`,
      { visible: true }
    );
    const rowCountDataBeforeDelete = await getRowCountData(page, masterDataViewId);
    expect(rowCountDataBeforeDelete.selectedRow).toBe("2");
    expect(rowCountDataBeforeDelete.rowCount).toBe("3");
    await deleteRowButton.click();

    const yesRowButton = await page.waitForXPath(
      `//button[@id='yesButton']`,
      { visible: true }
    );
    await yesRowButton.click();

    // await page.$eval("#saveButton", elem => elem .click())
    // await page.waitForSelector('#saveButton :not(.isRed)');

    await sleep(300);
    const rowCountDataAfterDelete = await getRowCountData(page, masterDataViewId);
    expect(rowCountDataAfterDelete.selectedRow).toBe("2");
    expect(rowCountDataAfterDelete.rowCount).toBe("2");
    // const imageAfterDeleting = await getImage(page, scroller);
    // expect(imageAfterDeleting).toMatchImageSnapshot();


    //duplicate first row
    await page.mouse.click(
      box.x + 50,
      box.y + 15
    );

    const copyRowButton = await page.waitForXPath(
      `//div[@id='${masterDataViewId}']//div[${xPathContainsClass("copyRow")}]`,
      { visible: true }
    );
    await copyRowButton.click();

    await sleep(300);
    const rowCountDataAfterDuplication = await getRowCountData(page, masterDataViewId);
    expect(rowCountDataAfterDuplication.selectedRow).toBe("3");
    expect(rowCountDataAfterDuplication.rowCount).toBe("3");
    // const imageAfterCopying = await getImage(page, scroller);
    // expect(imageAfterCopying).toMatchImageSnapshot();

    await refreshAndThrowChangesAway();
    await sleep(500);

    // await sleep(120 * 1000);
  });
  it("Should perform basic master detail interaction", async () => {
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        sectionsMenuItemId,
        masterDerailMenuItemId,
      ]);
    // Add rows to master
    await page.waitForXPath(
      `//div[@id='${masterDataViewId}']//div[${xPathContainsClass("addRow")}]`,
      { visible: true }
    );
    await addRowToMaster("str11", "str12");
    await addRowToMaster("str21", "str22");

    // Add a row to detail (second line in the master active)
    const detailTabHandle = await page.$(`#${detailTabHandelId}`);
    await detailTabHandle.click();

    const formPerspectiveButton = await page.$(`#${detailDataViewId} .formPerspectiveButton`);
    await formPerspectiveButton.click();

    await addRowToDetail("detail2");

    await sleep(500);
    await selectMasterRow(0);
    await sleep(500);

    let detailRowCountData = await getRowCountData(page, detailDataViewId);
    expect(detailRowCountData.selectedRow).toBe("-");
    expect(detailRowCountData.rowCount).toBe("0");

    await sleep(500);
    await selectMasterRow(1);

    detailRowCountData = await getRowCountData(page, detailDataViewId);
    expect(detailRowCountData.selectedRow).toBe("1");
    expect(detailRowCountData.rowCount).toBe("1");

    await refreshAndThrowChangesAway();
  });
});
