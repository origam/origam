const { test, expect } = require("@playwright/test");
const { createHash } = require("crypto");
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
  masterDataViewId,
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

async function getSectionPanel(page, tabHandleId) {
  const tabHandle = page.locator(`#${tabHandleId}`);
  const tabIndex = await tabHandle.evaluate(handle =>
    Array.from(handle.parentElement.children).indexOf(handle)
  );
  // Handles and panels have matching order in adjacent containers.
  return tabHandle.locator("..").locator("xpath=following-sibling::*[1]/*").nth(tabIndex);
}

async function getScrollPosition(page, dataViewId) {
  return page.locator(`#${dataViewId} .horiz-scrollbar`).evaluate(element => ({
    left: element.scrollLeft,
    top: element.scrollTop,
  }));
}

async function getSplitterPanelSizes(splitter) {
  return splitter.locator(':scope > [class*="Splitter-module__panel"]').evaluateAll(panels =>
    panels.map(panel => {
      const bounds = panel.getBoundingClientRect();
      return { width: bounds.width, height: bounds.height };
    })
  );
}

async function getCanvasContents(page, dataViewId, tabToActivate) {
  const tabHandle = await tabToActivate?.elementHandle();
  const contents = await page.evaluate(async ({ dataViewId, tabHandle }) => {
    if (tabHandle) {
      tabHandle.click();
      // Capture the incoming frame before readiness waits allow a delayed repaint.
      await new Promise(requestAnimationFrame);
    }
    const dataView = document.getElementById(dataViewId);
    if (!dataView) {
      return [];
    }
    if (tabHandle && getComputedStyle(dataView).visibility !== "visible") {
      throw new Error("The table was not visible in the first activation frame.");
    }
    const canvases = Array.from(dataView.querySelectorAll("canvas"));
    return canvases.map(canvas => {
      const dimensions = { width: canvas.width, height: canvas.height };
      if (canvas.width === 0 || canvas.height === 0) {
        return { ...dimensions, hasNonWhitePixels: false, image: null };
      }
      const context = canvas.getContext("2d");
      const pixels = context.getImageData(0, 0, canvas.width, canvas.height).data;
      let hasNonWhitePixels = false;
      for (let index = 0; index < pixels.length; index += 4) {
        if (
          pixels[index + 3] !== 0 &&
          (pixels[index] < 255 || pixels[index + 1] < 255 || pixels[index + 2] < 255)
        ) {
          hasNonWhitePixels = true;
          break;
        }
      }
      return { ...dimensions, hasNonWhitePixels, image: canvas.toDataURL() };
    });
  }, { dataViewId, tabHandle });
  // Hash in Node so this also works against HTTP test servers without Web Crypto.
  return contents.map(({ image, ...canvas }) => ({
    ...canvas,
    pixelHash: image === null ? null : createHash("sha256").update(image).digest("hex"),
  }));
}

test.describe("Html client", () => {
  // Verify getCanvasContents itself using controlled drawings on a mocked page.
  // Playwright fulfills canvas.test without an external server; HTTP also checks
  // that canvas comparison works without browser Web Crypto.
  test("Should reject blank canvas contents and detect missing cell text", async ({ page }) => {
    await page.waitForLoadState("networkidle");
    await page.route("http://canvas.test/", route => route.fulfill({
      contentType: "text/html",
      body: '<div id="canvas-content-test"><canvas width="200" height="100"></canvas></div>',
    }));
    await page.goto("http://canvas.test/");
    const canvas = page.locator("canvas");
    expect((await getCanvasContents(page, "canvas-content-test"))[0].hasNonWhitePixels).toBe(false);
    await canvas.evaluate(canvas => {
      const context = canvas.getContext("2d");
      context.fillStyle = "white";
      context.fillRect(0, 0, canvas.width, canvas.height);
    });
    const blankContents = await getCanvasContents(page, "canvas-content-test");
    expect(blankContents[0].hasNonWhitePixels).toBe(false);

    await canvas.evaluate(canvas => {
      const context = canvas.getContext("2d");
      context.strokeStyle = "gray";
      context.strokeRect(10, 10, 180, 80);
    });
    const gridOnlyContents = await getCanvasContents(page, "canvas-content-test");
    await canvas.evaluate(canvas => {
      const context = canvas.getContext("2d");
      context.fillStyle = "black";
      context.fillText("Table cell", 20, 40);
    });
    const cellContents = await getCanvasContents(page, "canvas-content-test");
    expect(cellContents[0].hasNonWhitePixels).toBe(true);
    expect(cellContents).not.toEqual(blankContents);
    expect(cellContents).not.toEqual(gridOnlyContents);
  });

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
    const canvasContents = await getCanvasContents(page, allDataTypesLazyDataViewId);
    expect(canvasContents.length).toBeGreaterThan(0);
    for (const canvas of canvasContents) {
      expect(canvas.width).toBeGreaterThan(0);
      expect(canvas.height).toBeGreaterThan(0);
      expect(canvas.hasNonWhitePixels).toBe(true);
    }

    await openMenuItem(page, [topMenuHeader, sectionsMenuItemId, masterDetailLazyLoadedMenuItemId]);

    await expect(dataView).toBeHidden();
    expect(await getScrollPosition(page, allDataTypesLazyDataViewId)).toEqual(scrollPosition);
    expect(await getCanvasContents(page, allDataTypesLazyDataViewId)).toEqual(canvasContents);

    await activateSectionTab(page, detailTabHandelId);
    const detailDataView = page.locator(`#${detailDataViewId}`);
    await expect(detailDataView).toBeVisible();

    const screenTab = page
      .locator('[class*="TabbedViewHandle-module__root"]')
      .filter({ hasText: /^All Data Types Lazy Loaded\s*$/ });
    const firstFrameContents = await getCanvasContents(page, allDataTypesLazyDataViewId, screenTab);
    expect(firstFrameContents).toEqual(canvasContents);
    await expect(screenTab).toHaveClass(/\bisActive\b/);
    await expect(dataView).toBeVisible();
    await expect(detailDataView).toBeHidden();
    await waitForApplicationReady(page);
    await getTableData(page, allDataTypesLazyDataViewId);
    await expect
      .poll(() => getScrollPosition(page, allDataTypesLazyDataViewId))
      .toEqual(scrollPosition);
    expect(await getCanvasContents(page, allDataTypesLazyDataViewId)).toEqual(canvasContents);
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

    const nestedPanel = await getSectionPanel(page, detailsDetailTabHandleId);
    const nestedLayout = await nestedPanel.evaluate(panel => {
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
    const tabSwitch = await nestedPanel
      .evaluate(async (previousPanel, nextTabHandleId) => {
        const tabHandle = document.getElementById(nextTabHandleId);
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
      }, detailTabHandelId);
    expect(tabSwitch.isActive).toBe(true);
    expect(tabSwitch.visibleBefore).toHaveLength(2);
    expect(tabSwitch.visibleAfter).toEqual([]);
    await waitForApplicationReady(page);
    await expect(detailEditor).toBeVisible();
    await detailEditor.focus();
    await expect(detailEditor).toBeFocused();
  });

  test("Should retain resized splitter panels across section and screen switches", async ({
    page,
  }) => {
    await login(page);
    await openMenuItem(page, [
      topMenuHeader,
      widgetsMenuItemId,
      sectionsMenuItemId,
      masterDetailLazyLoadedMenuItemId,
    ]);

    const masterDataView = page.locator(`#${masterDataViewId}`);
    const splitter = masterDataView.locator(
      'xpath=ancestor::*[contains(@class, "Splitter-module__root")][1]'
    );
    const divider = splitter.locator(':scope > [class*="Splitter-module__divider"]');
    const initialSizes = await getSplitterPanelSizes(splitter);
    expect(initialSizes).toHaveLength(2);
    const dragDistance = initialSizes[0].height > initialSizes[1].height ? -80 : 80;
    const bounds = await divider.boundingBox();
    expect(bounds).not.toBeNull();
    const x = bounds.x + bounds.width / 2;
    const y = bounds.y + bounds.height / 2;
    await page.mouse.move(x, y);
    await page.mouse.down();
    await page.mouse.move(x, y + dragDistance, { steps: 5 });
    await page.mouse.up();
    await expect.poll(async () => (await getSplitterPanelSizes(splitter))[0].height)
      .toBeCloseTo(initialSizes[0].height + dragDistance, 0);
    await waitForApplicationReady(page);
    const resizedSizes = await getSplitterPanelSizes(splitter);
    expect(resizedSizes[1].height).toBeCloseTo(initialSizes[1].height - dragDistance, 0);

    await activateSectionTab(page, detailTabHandelId);
    await activateSectionTab(page, detailsDetailTabHandleId);
    await activateSectionTab(page, detailTabHandelId);
    expect(await getSplitterPanelSizes(splitter)).toEqual(resizedSizes);

    await openMenuItem(page, [topMenuHeader, allDataTypesLazyMenuItemsId]);
    await expect(masterDataView).toBeHidden();
    expect(await getSplitterPanelSizes(splitter)).toEqual(resizedSizes);

    const screenTab = page.locator('[class*="TabbedViewHandle-module__root"]')
      .filter({ hasText: /^Master-Detail Lazy Loaded\s*$/ });
    await screenTab.click();
    await expect(masterDataView).toBeVisible();
    expect(await getSplitterPanelSizes(splitter)).toEqual(resizedSizes);
    await waitForApplicationReady(page);
    expect(await getSplitterPanelSizes(splitter)).toEqual(resizedSizes);
  });
});
