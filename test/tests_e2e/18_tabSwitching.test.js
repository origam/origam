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
  test("Should hide animated controls on the first frame of screen and section switches", async ({
    page,
  }) => {
    await login(page);
    // Use the application's compiled styles to cover controls absent from this test model.
    const switches = await page.evaluate(async () => {
      const rules = Array.from(document.styleSheets).flatMap(sheet => Array.from(sheet.cssRules));
      function styleClass(moduleName, localName) {
        const prefix = `.${moduleName}-module__${localName}__`;
        const rule = rules.find(rule =>
          rule.selectorText?.startsWith(prefix) && /^\.[\w-]+$/.test(rule.selectorText)
        );
        if (!rule) {
          throw new Error(`Missing application style: ${moduleName}.${localName}`);
        }
        return rule.selectorText.slice(1);
      }

      const results = [];
      for (const moduleName of ["Screen", "TabbedViewPanel"]) {
        const panel = document.createElement("div");
        panel.className = styleClass(moduleName, "root");
        panel.classList.toggle("isActive", moduleName === "TabbedViewPanel");
        document.body.append(panel);

        function addControl(name, moduleName, localName, tagName = "div") {
          const control = document.createElement(tagName);
          control.className = styleClass(moduleName, localName);
          control.dataset.control = name;
          panel.append(control);
          return control;
        }

        const checklist = addControl("checklist", "CheckList", "root");
        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.dataset.control = "checkbox";
        checklist.append(checkbox);
        const mapSearch = addControl("map search", "MapPerspectiveUI", "mapSearch");
        const mapInput = document.createElement("input");
        mapInput.className = "input";
        mapInput.dataset.control = "map input";
        mapSearch.append(mapInput);
        addControl("filter input", "FilterSettingsCommon", "input", "input");
        addControl("filter operator", "FilterSettingsComboBox", "trigger", "button");
        addControl("column button", "ColumnsDialog", "button", "button");

        const controls = Array.from(panel.querySelectorAll("[data-control]"));
        function visibleControls() {
          return controls
            .filter(control => getComputedStyle(control).visibility === "visible")
            .map(control => control.dataset.control);
        }

        const visibleBefore = visibleControls();
        await new Promise(requestAnimationFrame);
        panel.classList.toggle("isHidden", moduleName === "Screen");
        panel.classList.remove("isActive");
        await new Promise(requestAnimationFrame);
        const visibleAfter = visibleControls();
        panel.remove();
        results.push({ moduleName, visibleBefore, visibleAfter });
      }
      return results;
    });

    for (const result of switches) {
      expect(result.visibleBefore, result.moduleName).toHaveLength(7);
      expect(result.visibleAfter, result.moduleName).toEqual([]);
    }
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
