const { sleep, openMenuItem, login, getRowCountData, waitForRowCountData, afterEachTest,
  beforeEachTest } = require("./testTools");
const { widgetsMenuItemId, allDataTypesDataViewId, selectionDialogAutoRefreshMenuItemId,
  userAccountSelectionDialogDataViewId } = require("./modelIds");
const { restoreAllDataTypesTable } = require("./dbTools");

const autoRefreshIntervalMilliseconds = 3_000;
const screenRowCount = 30;
const enteredDate = "23.06.2021";
const selectionDialogDateInputSelector = `#${userAccountSelectionDialogDataViewId} input`;

let browser;
let page;
let sessionRequests;

beforeAll(async () => {
  await restoreAllDataTypesTable();
});

beforeEach(async () => {
  [browser, page] = await beforeEachTest();
  sessionRequests = trackSessionRequests();
});

afterEach(async () => {
  await afterEachTest(browser);
  browser = undefined;
});

function trackSessionRequests() {
  const trackedRequests = { initializedSessionIds: [], destroyedSessionIds: [], refreshResponses: [] };
  page.on("request", request => {
    if (request.url().endsWith("/UIService/DestroyUI")) {
      trackedRequests.destroyedSessionIds.push(JSON.parse(request.postData()).FormSessionId);
    }
  });
  page.on("response", async response => {
    const url = response.url();
    if (url.includes("/UIService/RefreshData/")) {
      trackedRequests.refreshResponses.push({
        sessionId: new URL(url).pathname.split("/").pop(),
        status: response.status(),
      });
    } else if (url.endsWith("/UIService/InitUI")) {
      const body = await response.json();
      trackedRequests.initializedSessionIds.push(body.sessionId);
    }
  });
  return trackedRequests;
}

function countRefreshes(sessionId) {
  return sessionRequests.refreshResponses
    .filter(refreshResponse => refreshResponse.sessionId === sessionId)
    .length;
}

async function waitUntil(condition, timeoutMilliseconds, description) {
  const deadline = Date.now() + timeoutMilliseconds;
  while (Date.now() < deadline) {
    if (await condition()) {
      return;
    }
    await sleep(100);
  }
  throw new Error(`Timed out waiting until ${description}`);
}

async function getSelectionDialogDate() {
  return page.$eval(selectionDialogDateInputSelector, input => input.value);
}

async function openSelectionDialog() {
  await login(page);
  await openMenuItem(page, [widgetsMenuItemId, selectionDialogAutoRefreshMenuItemId]);
  await page.waitForSelector(selectionDialogDateInputSelector, { visible: true });
}

async function enterSelectionDialogDate() {
  const input = await page.$(selectionDialogDateInputSelector);
  await input.click({ clickCount: 3 });
  await page.keyboard.type(enteredDate);
  await page.keyboard.press("Enter");
  await waitUntil(
    async () => (await getSelectionDialogDate()).startsWith(enteredDate),
    10_000,
    "the date is entered"
  );
  return getSelectionDialogDate();
}

async function confirmSelectionDialog() {
  const okButton = await page.waitForXPath(
    `//*[@id='modal-window-portal']//*[self::button or @role='button'][normalize-space(.)='OK']`,
    { visible: true }
  );
  await okButton.click();
  await waitForRowCountData(page, allDataTypesDataViewId, screenRowCount);
}

async function isFocusInsideScreen() {
  return page.evaluate(
    dataViewId => document.getElementById(dataViewId).contains(document.activeElement),
    allDataTypesDataViewId
  );
}

async function isSessionExpiredMessageShown() {
  return page.evaluate(() => document.body.innerText.includes("Your connection expired"));
}

describe("Html client", () => {
  it("Should keep values entered in a selection dialog while the auto refresh interval passes", async () => {
    await openSelectionDialog();
    const enteredValue = await enterSelectionDialogDate();

    await sleep(autoRefreshIntervalMilliseconds + 1_500);

    expect(await getSelectionDialogDate()).toBe(enteredValue);
    expect(sessionRequests.refreshResponses).toEqual([]);
  }, 120_000);

  it("Should stop refreshing a selection dialog once it opens its screen", async () => {
    await openSelectionDialog();
    await enterSelectionDialogDate();
    await confirmSelectionDialog();

    const [selectionDialogSessionId, screenSessionId] = sessionRequests.initializedSessionIds;
    expect(sessionRequests.destroyedSessionIds).toEqual([selectionDialogSessionId]);
    await waitUntil(
      () => countRefreshes(screenSessionId) >= 2,
      4 * autoRefreshIntervalMilliseconds,
      "the screen is refreshed twice"
    );
    expect(sessionRequests.refreshResponses).toEqual(
      sessionRequests.refreshResponses.map(() => ({ sessionId: screenSessionId, status: 200 }))
    );
    expect(await isSessionExpiredMessageShown()).toBe(false);
  }, 120_000);

  it("Should keep the selected row and focus after an auto refresh of a screen opened from a selection dialog", async () => {
    await openSelectionDialog();
    await confirmSelectionDialog();
    const screenSessionId = sessionRequests.initializedSessionIds[1];

    const tableArea = await page.$(`#${allDataTypesDataViewId} [class*='_cellAreaContainer']`);
    const box = await tableArea.boundingBox();
    await page.mouse.click(box.x + 50, box.y + 100);
    await waitUntil(
      async () => (await getRowCountData(page, allDataTypesDataViewId)).selectedRow !== "1",
      10_000,
      "a row other than the first one is selected"
    );
    const { selectedRow } = await getRowCountData(page, allDataTypesDataViewId);
    expect(await isFocusInsideScreen()).toBe(true);

    const refreshCountBeforeWaiting = countRefreshes(screenSessionId);
    await waitUntil(
      () => countRefreshes(screenSessionId) >= refreshCountBeforeWaiting + 2,
      4 * autoRefreshIntervalMilliseconds,
      "the screen is refreshed twice"
    );
    await sleep(500);

    expect(await getRowCountData(page, allDataTypesDataViewId))
      .toEqual({ rowCount: String(screenRowCount), selectedRow });
    expect(await isFocusInsideScreen()).toBe(true);
    expect(await isSessionExpiredMessageShown()).toBe(false);
  }, 120_000);
});
