const {
  sleep,
  openMenuItem,
  login,
  waitForRowCountData,
  beforeEachTest,
  afterEachTest,
} = require("./testTools");
const {
  topMenuHeader,
  widgetsMenuItemId,
  sectionsMenuItemId,
  allDataTypesLazyMenuItemsId,
  allDataTypesLazyDataViewId,
  masterDetailLazyLoadedMenuItemId,
  detailDataViewId,
  detailEditorId,
  detailTabHandelId,
  detailsDetailTabHandleId,
} = require("./modelIds");
const {
  restoreAllDataTypesTable,
  restoreWidgetSectionTestMaster,
  clearScreenConfiguration,
} = require("./dbTools");

let browser;
let page;

beforeAll(async () => {
  await restoreAllDataTypesTable();
  await restoreWidgetSectionTestMaster();
  await clearScreenConfiguration();
});

beforeEach(async () => {
  [browser, page] = await beforeEachTest();
});

afterEach(async () => {
  await afterEachTest(browser);
  browser = undefined;
});

async function activateSectionTab(tabHandleId) {
  const tabHandle = await page.waitForSelector(`#${tabHandleId}`, {
    visible: true,
  });
  await tabHandle.click();
  await page.waitForFunction(
    id => document.getElementById(id).classList.contains("isActive"),
    {},
    tabHandleId
  );
}

async function activateScreenTab(title) {
  const [tabHandle] = await page.$x(`//div[@title="${title}"]`);
  if (!tabHandle) {
    throw new Error(`Screen tab '${title}' was not found.`);
  }
  await tabHandle.click();
}

async function getScrollPosition(dataViewId) {
  return page.$eval(`#${dataViewId} .horiz-scrollbar`, element => ({
    left: element.scrollLeft,
    top: element.scrollTop,
  }));
}

async function canvasHasRenderedPixels(dataViewId) {
  return page.$$eval(`#${dataViewId} canvas`, canvases =>
    canvases.some(canvas => {
      if (canvas.width === 0 || canvas.height === 0) {
        return false;
      }
      const context = canvas.getContext("2d");
      return context && context.getImageData(0, 0, 1, 1).data[3] !== 0;
    })
  );
}

describe("Html client", () => {
  it("Should preserve tab contents while excluding inactive tabs from interaction", async () => {
    await login(page);
    await openMenuItem(page, [
      topMenuHeader,
      widgetsMenuItemId,
      allDataTypesLazyMenuItemsId,
    ]);
    await waitForRowCountData(page, allDataTypesLazyDataViewId, 2099);

    await page.$eval(
      `#${allDataTypesLazyDataViewId} .horiz-scrollbar`,
      element => {
        element.scrollTo({left: 200, top: 600});
      }
    );
    await page.waitForFunction(
      dataViewId => {
        const scroller = document.querySelector(
          `#${dataViewId} .horiz-scrollbar`
        );
        return scroller.scrollLeft > 0 && scroller.scrollTop > 0;
      },
      {},
      allDataTypesLazyDataViewId
    );
    await sleep(100);
    const scrollPosition = await getScrollPosition(allDataTypesLazyDataViewId);
    expect(scrollPosition.left).toBeGreaterThan(0);
    expect(scrollPosition.top).toBeGreaterThan(0);
    expect(await canvasHasRenderedPixels(allDataTypesLazyDataViewId)).toBe(true);

    await openMenuItem(page, [
      topMenuHeader,
      widgetsMenuItemId,
      sectionsMenuItemId,
      masterDetailLazyLoadedMenuItemId,
    ]);

    await activateSectionTab(detailTabHandelId);
    const formPerspectiveButton = await page.waitForSelector(
      `#${detailDataViewId} .formPerspectiveButton`,
      {visible: true}
    );
    await formPerspectiveButton.click();
    await page.waitForSelector(`#${detailEditorId}`, {visible: true});

    await activateSectionTab(detailsDetailTabHandleId);

    const hiddenEditorCanReceiveFocus = await page.$eval(
      `#${detailEditorId}`,
      editor => {
        editor.focus();
        return document.activeElement === editor;
      }
    );
    expect(hiddenEditorCanReceiveFocus).toBe(false);

    const splitterLayout = await page.$eval(
      `#${detailsDetailTabHandleId}`,
      tabHandle => {
        const handleRow = tabHandle.parentElement;
        const tabIndex = Array.from(handleRow.children).indexOf(tabHandle);
        const panel = handleRow.nextElementSibling.children[tabIndex];
        const splitter = Array.from(panel.querySelectorAll(".isHoriz, .isVert"))
          .find(element =>
            Array.from(element.children).some(child =>
              child.firstElementChild?.classList.contains("dividerLine")
            )
          );
        const divider = Array.from(splitter.children)
          .find(child => child.firstElementChild?.classList.contains("dividerLine"));
        const panels = Array.from(splitter.children).filter(child => child !== divider);
        const rectangle = element => {
          const bounds = element.getBoundingClientRect();
          return {height: bounds.height, width: bounds.width};
        };
        return {
          divider: rectangle(divider),
          panels: panels.map(rectangle),
          splitter: rectangle(splitter),
        };
      }
    );
    expect(splitterLayout.splitter.width).toBeGreaterThan(0);
    expect(splitterLayout.splitter.height).toBeGreaterThan(0);
    expect(splitterLayout.divider.width).toBeGreaterThan(0);
    expect(splitterLayout.divider.height).toBeGreaterThan(0);
    expect(splitterLayout.panels).toHaveLength(2);
    splitterLayout.panels.forEach(panel => {
      expect(panel.width).toBeGreaterThan(0);
      expect(panel.height).toBeGreaterThan(0);
    });

    await activateScreenTab("All Data Types Lazy Loaded");
    await page.waitForFunction(
      dataViewId => {
        const dataView = document.getElementById(dataViewId);
        return dataView && getComputedStyle(dataView).visibility !== "hidden";
      },
      {},
      allDataTypesLazyDataViewId
    );
    await sleep(100);

    expect(await getScrollPosition(allDataTypesLazyDataViewId)).toEqual(scrollPosition);
    expect(await canvasHasRenderedPixels(allDataTypesLazyDataViewId)).toBe(true);
  });
});
