const { sleep, openMenuItem, login, waitForRowCount, waitForRowCountData, beforeEachTest, afterEachTest, getRowCountData
} = require('./testTools');
const {widgetsMenuItemId, allDataTypesMenuId, topMenuHeader,  allDataTypesDataViewId,
  detailTabHandelId,
  detailDataViewId,
  masterDataViewId
} = require("./modelIds");
const {restoreAllDataTypesTable, clearScreenConfiguration} = require("./dbTools");
const expect = require("expect");

let browser;
let page;

beforeAll(async() => {
  await restoreAllDataTypesTable();
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
const text1InputId ="editor_cb584956-8f34-4d95-852e-eff4680a2673";

async function refreshAndThrowChangesAway() {
  await page.$eval("#refreshButton", elem => elem.click());

  const dontSaveButton = await page.waitForXPath(
    `//button[@id='dontSaveButton']`,
    {visible: true}
  );
  await dontSaveButton.click();
}

async function throttleRequests(){
  const client = await page.target().createCDPSession();
  await client.send('Network.emulateNetworkConditions', {
    'offline': false,
    'downloadThroughput': 500 * 1024 / 8,
    'uploadThroughput': 500 * 1024 / 8,
    'latency': 20
  })
}

async function cancelRequestThrottling(){
  const client = await page.target().createCDPSession();
  await client.send('Network.emulateNetworkConditions', {
    'offline': false,
    'downloadThroughput': -1,
    'uploadThroughput': -1,
    'latency': 0
  })
}

describe("Html client", () => {
  it("Should fill characters typed before the new row in grid is rendered", async () => {
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        allDataTypesMenuId
      ]);

    await waitForRowCount(page, dataViewId,30);

    await throttleRequests();

    await page.$eval(`#${allDataTypesDataViewId} .addRow`, elem => elem.click());

    // The request throttling should cause these key presses to occur before the input in the
    // fist column is rendered and focused
    await page.keyboard.press("a");
    await page.keyboard.press("b");
    await page.keyboard.press("c");

    await page.waitForFunction(`document.activeElement == document.getElementById("${text1InputId}")`);

    await page.keyboard.press("d");
    await page.keyboard.press("e");
    await page.keyboard.press("f");

    let [input] = await page.$x(`//input[@id='${text1InputId}']`)
    const inputValue = await page.evaluate(x => x.value, input);
    expect(inputValue).toContain('abcdef');

    await cancelRequestThrottling();

    await refreshAndThrowChangesAway();
    await sleep(500);
  });
});

