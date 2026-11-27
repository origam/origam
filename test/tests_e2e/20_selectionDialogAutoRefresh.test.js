const { test, expect } = require("@playwright/test");
const {
  openMenuItem,
  login,
  beforeEachTest,
  getRowCountData,
  waitForRowCountData,
  waitForApplicationReady,
} = require("./testTools");
const {
  widgetsMenuItemId,
  allDataTypesDataViewId,
  selectionDialogAutoRefreshMenuItemId,
  dateSelectionDialogDataViewId,
} = require("./modelIds");
const { restoreAllDataTypesTable } = require("./dbTools");

const autoRefreshIntervalMilliseconds = 3_000;
const dateFrom = "23.06.2021";
const dateTo = "02.07.2021";
const rowCountInDateRange = 10;

test.beforeAll(async () => {
  await restoreAllDataTypesTable();
});

test.beforeEach(async ({ page }) => {
  await beforeEachTest(page);
});

function trackSessionRequests(page) {
  const sessionRequests = { initializedSessionIds: [], destroyedSessionIds: [], refreshResponses: [] };
  page.on("request", request => {
    if (request.url().endsWith("/UIService/DestroyUI")) {
      sessionRequests.destroyedSessionIds.push(JSON.parse(request.postData()).FormSessionId);
    }
  });
  page.on("response", async response => {
    const url = response.url();
    if (url.includes("/UIService/RefreshData/")) {
      sessionRequests.refreshResponses.push({
        sessionId: new URL(url).pathname.split("/").pop(),
        status: response.status(),
      });
    } else if (url.endsWith("/UIService/InitUI")) {
      const body = await response.json();
      sessionRequests.initializedSessionIds.push(body.sessionId);
    }
  });
  return sessionRequests;
}

function countRefreshes(sessionRequests, sessionId) {
  return sessionRequests.refreshResponses
    .filter(refreshResponse => refreshResponse.sessionId === sessionId)
    .length;
}

function selectionDialogDateInput(page, caption) {
  return page
    .locator(`#${dateSelectionDialogDataViewId} label`)
    .filter({ hasText: new RegExp(`^${caption}$`) })
    .locator("xpath=following-sibling::div[1]//input");
}

async function openSelectionDialog(page) {
  await login(page);
  await openMenuItem(page, [widgetsMenuItemId, selectionDialogAutoRefreshMenuItemId]);
  await expect(selectionDialogDateInput(page, "From")).toBeVisible();
}

async function enterDateRange(page) {
  for (const [caption, value] of [["From", dateFrom], ["To", dateTo]]) {
    const input = selectionDialogDateInput(page, caption);
    await input.fill(value);
    await input.press("Tab");
    await expect(input).toHaveValue(value);
  }
  await waitForApplicationReady(page);
}

async function confirmSelectionDialog(page) {
  await page.locator("#modal-window-portal").getByRole("button", { name: "OK", exact: true }).click();
  await waitForRowCountData(page, allDataTypesDataViewId, rowCountInDateRange);
}

test.describe("Html client", () => {
  test("Should keep values entered in a selection dialog while the auto refresh interval passes", async ({ page }) => {
    const sessionRequests = trackSessionRequests(page);
    await openSelectionDialog(page);
    await enterDateRange(page);

    await page.waitForTimeout(autoRefreshIntervalMilliseconds + 1_500);

    await expect(selectionDialogDateInput(page, "From")).toHaveValue(dateFrom);
    await expect(selectionDialogDateInput(page, "To")).toHaveValue(dateTo);
    expect(sessionRequests.refreshResponses).toEqual([]);
  });

  test("Should stop refreshing a selection dialog once it opens its screen", async ({ page }) => {
    const sessionRequests = trackSessionRequests(page);
    await openSelectionDialog(page);
    await enterDateRange(page);
    await confirmSelectionDialog(page);

    const [selectionDialogSessionId, screenSessionId] = sessionRequests.initializedSessionIds;
    expect(sessionRequests.destroyedSessionIds).toEqual([selectionDialogSessionId]);
    await expect.poll(
      () => countRefreshes(sessionRequests, screenSessionId),
      { timeout: 4 * autoRefreshIntervalMilliseconds }
    ).toBeGreaterThanOrEqual(2);
    expect(sessionRequests.refreshResponses).toEqual(
      sessionRequests.refreshResponses.map(() => ({ sessionId: screenSessionId, status: 200 }))
    );
    await expect(page.getByText("Your connection expired", { exact: false })).toHaveCount(0);
    await waitForRowCountData(page, allDataTypesDataViewId, rowCountInDateRange);
  });

  test("Should keep the selected row and focus after an auto refresh of a screen opened from a selection dialog", async ({ page }) => {
    const sessionRequests = trackSessionRequests(page);
    await openSelectionDialog(page);
    await enterDateRange(page);
    await confirmSelectionDialog(page);
    const screenSessionId = sessionRequests.initializedSessionIds[1];

    await page
      .locator(`#${allDataTypesDataViewId} [class*='_cellAreaContainer']`)
      .click({ position: { x: 50, y: 100 } });
    await expect.poll(
      async () => (await getRowCountData(page, allDataTypesDataViewId)).selectedRow
    ).not.toBe("1");
    const { selectedRow } = await getRowCountData(page, allDataTypesDataViewId);
    const isFocusInsideScreen = () => page.evaluate(
      dataViewId => document.getElementById(dataViewId).contains(document.activeElement),
      allDataTypesDataViewId
    );
    expect(await isFocusInsideScreen()).toBe(true);

    const refreshCountBeforeWaiting = countRefreshes(sessionRequests, screenSessionId);
    await expect.poll(
      () => countRefreshes(sessionRequests, screenSessionId),
      { timeout: 4 * autoRefreshIntervalMilliseconds }
    ).toBeGreaterThanOrEqual(refreshCountBeforeWaiting + 2);
    await waitForApplicationReady(page);

    const rowCountData = await getRowCountData(page, allDataTypesDataViewId);
    expect(rowCountData).toEqual({ rowCount: String(rowCountInDateRange), selectedRow });
    expect(await isFocusInsideScreen()).toBe(true);
  });
});
