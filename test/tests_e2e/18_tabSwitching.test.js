const { test, expect } = require("@playwright/test");
const {
  openMenuItem,
  login,
  waitForRowCountData,
  beforeEachTest,
  getTableData,
  performAndWaitForTableRender,
  waitForApplicationReady,
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

// Keep both scrollbars available even when all default columns fit the CI viewport.
test.use({ viewport: { width: 1000, height: 800 } });

test.beforeAll(async () => {
  await restoreAllDataTypesTable();
  await restoreWidgetSectionTestMaster();
  await clearScreenConfiguration();
});

test.beforeEach(async ({ page }) => {
  await beforeEachTest(page);
});

async function activateSectionTab(page, tabHandleId) {
  const tabHandle = page.locator(`#${tabHandleId}`);
  await tabHandle.click();
  await expect(tabHandle).toHaveClass(/\bisActive\b/);
  await waitForApplicationReady(page);
}

async function getScrollPosition(page, dataViewId) {
  return page.locator(`#${dataViewId} .horiz-scrollbar`).evaluate(element => ({
    left: element.scrollLeft,
    top: element.scrollTop,
  }));
}

async function canvasHasRenderedPixels(page, dataViewId) {
  return page.locator(`#${dataViewId} canvas`).evaluateAll(canvases =>
    canvases.some(canvas => {
      if (canvas.width === 0 || canvas.height === 0) {
        return false;
      }
      const context = canvas.getContext("2d");
      return context && context.getImageData(0, 0, 1, 1).data[3] !== 0;
    })
  );
}

test.describe("Html client", () => {
  test("Should preserve scroll position and canvas contents across screen switches", async ({
    page,
  }) => {
    await login(page);
    await openMenuItem(page, [topMenuHeader, widgetsMenuItemId, allDataTypesLazyMenuItemsId]);
    await waitForRowCountData(page, allDataTypesLazyDataViewId, 2099);

    const dataView = page.locator(`#${allDataTypesLazyDataViewId}`);
    await performAndWaitForTableRender(page, allDataTypesLazyDataViewId, async () => {
      await dataView.locator(".horiz-scrollbar").evaluate(element => {
        element.scrollTo({ left: 200, top: 600 });
      });
    });
    const scrollPosition = await getScrollPosition(page, allDataTypesLazyDataViewId);
    expect(scrollPosition.left).toBeGreaterThan(0);
    expect(scrollPosition.top).toBeGreaterThan(0);
    await expect.poll(() => canvasHasRenderedPixels(page, allDataTypesLazyDataViewId)).toBe(true);
    const canvasDimensions = await dataView.locator("canvas").evaluateAll(canvases =>
      canvases.map(canvas => ({
        width: canvas.width,
        height: canvas.height,
      }))
    );

    await openMenuItem(page, [topMenuHeader, sectionsMenuItemId, masterDetailLazyLoadedMenuItemId]);

    await expect(dataView).toBeHidden();
    expect(await getScrollPosition(page, allDataTypesLazyDataViewId)).toEqual(scrollPosition);
    expect(
      await dataView.locator("canvas").evaluateAll(canvases =>
        canvases.map(canvas => ({
          width: canvas.width,
          height: canvas.height,
        }))
      )
    ).toEqual(canvasDimensions);
    expect(await canvasHasRenderedPixels(page, allDataTypesLazyDataViewId)).toBe(true);

    await activateSectionTab(page, detailTabHandelId);
    const detailDataView = page.locator(`#${detailDataViewId}`);
    await expect(detailDataView).toBeVisible();

    const screenTab = page
      .locator('[class*="TabbedViewHandle-module__root"]')
      .filter({ hasText: /^All Data Types Lazy Loaded\s*$/ });
    await screenTab.click();
    await expect(screenTab).toHaveClass(/\bisActive\b/);
    await expect(dataView).toBeVisible();
    await expect(detailDataView).toBeHidden();
    await waitForApplicationReady(page);
    await getTableData(page, allDataTypesLazyDataViewId);
    await expect
      .poll(() => getScrollPosition(page, allDataTypesLazyDataViewId))
      .toEqual(scrollPosition);
    await expect.poll(() => canvasHasRenderedPixels(page, allDataTypesLazyDataViewId)).toBe(true);
  });

  test("Should exclude inactive sections from focus and preserve nested panel layout", async ({
    page,
  }) => {
    await login(page);
    await openMenuItem(page, [
      topMenuHeader,
      widgetsMenuItemId,
      sectionsMenuItemId,
      masterDetailLazyLoadedMenuItemId,
    ]);

    await activateSectionTab(page, detailTabHandelId);
    await page.locator(`#${detailDataViewId} .formPerspectiveButton`).click();
    const detailEditor = page.locator(`#${detailEditorId}`);
    await expect(detailEditor).toBeVisible();
    await detailEditor.focus();
    await expect(detailEditor).toBeFocused();

    await activateSectionTab(page, detailsDetailTabHandleId);
    await expect(detailEditor).toBeHidden();

    const hiddenEditorCanReceiveFocus = await detailEditor.evaluate(editor => {
      editor.focus();
      return document.activeElement === editor;
    });
    expect(hiddenEditorCanReceiveFocus).toBe(false);

    const nestedLayout = await page.locator(`#${detailsDetailTabHandleId}`).evaluate(tabHandle => {
      const handleRow = tabHandle.parentElement;
      const tabIndex = Array.from(handleRow.children).indexOf(tabHandle);
      const panel = handleRow.nextElementSibling.children[tabIndex];
      // This model's nested SplitPanel has FixedSize=true and renders as a VBox.
      const layout = panel.firstElementChild;
      function rectangle(element) {
        const bounds = element.getBoundingClientRect();
        return { height: bounds.height, width: bounds.width };
      }
      return {
        panels: Array.from(layout.children).map(rectangle),
        container: rectangle(layout),
      };
    });
    expect(nestedLayout.container.width).toBeGreaterThan(0);
    expect(nestedLayout.container.height).toBeGreaterThan(0);
    expect(nestedLayout.panels).toHaveLength(2);
    nestedLayout.panels.forEach(panel => {
      expect(panel.width).toBeGreaterThan(0);
      expect(panel.height).toBeGreaterThan(0);
    });

    // Sample the outgoing buttons on the first frame; readiness waits or retrying
    // assertions would let a visibility transition finish and miss the overlap.
    const tabSwitch = await page
      .locator(`#${detailTabHandelId}`)
      .evaluate(async (tabHandle, previousTabHandleId) => {
        const previousTabHandle = document.getElementById(previousTabHandleId);
        const handleRow = previousTabHandle.parentElement;
        const tabIndex = Array.from(handleRow.children).indexOf(previousTabHandle);
        const previousPanel = handleRow.nextElementSibling.children[tabIndex];
        const buttons = Array.from(previousPanel.querySelectorAll("button"));
        await Promise.all(
          buttons.flatMap(button =>
            button.getAnimations().map(animation => animation.finished.catch(() => {}))
          )
        );

        function visibleButtonLabels() {
          return buttons
            .filter(button => getComputedStyle(button).visibility === "visible")
            .map(button => button.textContent);
        }

        const visibleBefore = visibleButtonLabels();
        tabHandle.click();
        await new Promise(requestAnimationFrame);
        return {
          isActive: tabHandle.classList.contains("isActive"),
          visibleBefore,
          visibleAfter: visibleButtonLabels(),
        };
      }, detailsDetailTabHandleId);
    expect(tabSwitch.isActive).toBe(true);
    expect(tabSwitch.visibleBefore).toHaveLength(2);
    expect(tabSwitch.visibleAfter).toEqual([]);
    await waitForApplicationReady(page);
    await expect(detailEditor).toBeVisible();
    await detailEditor.focus();
    await expect(detailEditor).toBeFocused();
  });
});
