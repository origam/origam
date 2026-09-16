const { expect } = require("@playwright/test");
const { userName, password } = require("./additionalConfig");

async function beforeEachTest(page) {
  await page.addInitScript(() => {
    localStorage.setItem("debugCloseAllForms", "1");
    localStorage.setItem("debugApplicationState", "1");
  });
  await page.goto("/");
}

async function getApplicationWorkState(page) {
  return page.evaluate(() => window.origamDebug?.getApplicationWorkState() ?? ({
    ready: false,
    reasons: ["debug-monitor:unavailable"],
  }));
}

async function waitForApplicationReady(page, timeout = 120_000) {
  let stablePollCount = 0;
  await expect.poll(async () => {
    const state = await getApplicationWorkState(page);
    stablePollCount = state.ready ? stablePollCount + 1 : 0;
    return stablePollCount >= 2 ? state : undefined;
  }, { intervals: [100, 250, 500], timeout }).toEqual({ ready: true, reasons: [] });
}

async function login(page) {
  await page.waitForLoadState("networkidle", { timeout: 60_000 });
  await page.locator("#userNameInput").fill(userName);
  await page.locator("#passInput").fill(password);
  const loginButton = page.locator("#loginButton");
  const menuHeader = page.locator("#menuHeader");
  await loginButton.evaluate(element => element.click());
  await expect(menuHeader).toBeVisible({ timeout: 120_000 });
  await waitForApplicationReady(page);
}

async function openMenuItem(page, menuItemIdList) {
  for (const menuItemId of menuItemIdList) await page.locator(`#${menuItemId}`).click();
  await waitForApplicationReady(page);
}

function modelInstanceId(dataViewId) {
  return dataViewId.startsWith("dataView_") ? dataViewId.slice(9) : dataViewId;
}

async function getTableData(page, dataViewId) {
  const id = modelInstanceId(dataViewId);
  let tableData;
  await expect.poll(async () => {
    tableData = await page.evaluate(key => window.tableDebugMonitor?.[key], id);
    const workState = await getApplicationWorkState(page);
    return Boolean(tableData?.rendered && workState.ready);
  }, { timeout: 60_000 }).toBe(true);
  await page.evaluate(() => { window.tableDebugMonitor = undefined; });
  return tableData;
}

async function waitForRowCount(page, dataViewId, expectedRowCount) {
  const id = modelInstanceId(dataViewId);
  let tableData;
  await expect.poll(async () => {
    tableData = await page.evaluate(key => window.tableDebugMonitor?.[key], id);
    const workState = await getApplicationWorkState(page);
    return tableData?.rendered && workState.ready ? tableData.data.length : undefined;
  }, { timeout: 60_000 }).toBe(expectedRowCount);
  await waitForDataViewLoadingToFinish(page, dataViewId);
  await page.evaluate(() => { window.tableDebugMonitor = undefined; });
  return tableData;
}

async function getRowCountData(page, dataViewId) {
  const text = await page.locator(`#${dataViewId} .rowCount`).innerText({ timeout: 120_000 });
  const [selectedRow, rowCount] = text.split("/").map(value => value.trim());
  return { rowCount, selectedRow };
}

async function waitForRowCountData(page, dataViewId, expectedRowCount) {
  let result;
  let stablePollCount = 0;
  await expect.poll(async () => {
    result = await getRowCountData(page, dataViewId);
    const loadingFinished = await page.evaluate(dataViewId => {
      const loadingIndicators = Array.from(document.querySelectorAll(
        `#${dataViewId} [class*="DataViewLoading-module__container"]`
      ));
      return loadingIndicators.every(element => element.offsetParent === null);
    }, dataViewId);
    const workState = await getApplicationWorkState(page);
    stablePollCount = loadingFinished && workState.ready && result.rowCount === String(expectedRowCount)
      ? stablePollCount + 1
      : 0;
    return stablePollCount >= 3 ? result.rowCount : undefined;
  }, { intervals: [250, 500, 1_000], timeout: 120_000 }).toBe(String(expectedRowCount));
  await page.evaluate(() => { window.tableDebugMonitor = undefined; });
  return result;
}

async function waitForDataViewLoadingToFinish(page, dataViewId) {
  const loadingIndicators = page.locator(
    `#${dataViewId} [class*="DataViewLoading-module__container"]`
  );
  await expect.poll(async () => loadingIndicators.evaluateAll(elements =>
    elements.every(element => element.offsetParent === null)
  ), { timeout: 120_000 }).toBe(true);
}

async function waitForRowSelected(page, dataViewId, expectedRow, timeout = 60_000) {
  let result;
  await expect.poll(async () => {
    result = await getRowCountData(page, dataViewId);
    return result.selectedRow;
  }, { timeout }).toBe(String(expectedRow));
  return result;
}

async function switchToFormPerspective({ page, aPropertyId }) {
  await page.locator(".formPerspectiveButton").click();
  await expect(page.locator(`#editor_${aPropertyId}`)).toBeVisible();
}

async function switchToTablePerspective({ page }) {
  await page.locator(".tablePerspectiveButton").click();
  await expect(page.locator(".tablePerspectiveDirector")).toBeVisible();
}

async function inputByPressingKeys({ page, value }) { await page.keyboard.type(value); }

async function switchLanguageTo({ page, locale }) {
  const languageLink = page.locator(`#languageLinkContainer a[value="${locale}"]`);
  await expect(languageLink).toBeVisible({ timeout: 120_000 });
  await Promise.all([
    page.waitForNavigation({ waitUntil: "domcontentloaded", timeout: 120_000 }),
    languageLink.evaluate((element, locale) => {
      const form = document.getElementById("languageSelectForm");
      const input = document.createElement("input");
      input.type = "hidden";
      input.name = "culture";
      input.value = locale;
      form.append(input);
      form.submit();
    }, locale),
  ]);
  await expect.poll(async () => {
    const cookies = await page.context().cookies();
    const localeCookie = cookies.find(cookie => cookie.name === "origamCurrentLocale");
    return localeCookie
      ? decodeURIComponent(localeCookie.value).startsWith(`c=${locale}|`)
      : false;
  }, { timeout: 120_000 }).toBe(true);
  await expect(page.locator("#loginButton")).toBeVisible({ timeout: 120_000 });
}

async function waitForFocus({ page, elementId }) {
  await expect(page.locator(`#${elementId}`)).toBeFocused();
}

module.exports = {
  beforeEachTest,
  getRowCountData,
  getTableData,
  inputByPressingKeys,
  login,
  openMenuItem,
  switchLanguageTo,
  switchToFormPerspective,
  switchToTablePerspective,
  waitForFocus,
  waitForApplicationReady,
  waitForRowCount,
  waitForRowCountData,
  waitForRowSelected,
};
