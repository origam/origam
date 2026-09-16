const { test, expect } = require("@playwright/test");
const { openMenuItem, login, waitForRowCountData, switchToFormPerspective,
  inputByPressingKeys, switchLanguageTo, waitForFocus,
  switchToTablePerspective, beforeEachTest
} = require('./testTools');
const {widgetsMenuItemId,topMenuHeader,
  allDataTypesLazyMenuItemsId} = require("./modelIds");
const { putNumericTestDataToAllDataTypes, clearScreenConfiguration,
  restoreAllDataTypesTable} = require("./dbTools");

let page;

test.beforeAll(async() => {
  await clearScreenConfiguration();
});

test.beforeEach(async ({ page: testPage }) => {
  page = testPage;
  await beforeEachTest(page);
});



const dataViewId = "dataView_e67865b0-ce91-413c-bed7-1da486399633";
const currencyPropertyId = "ff303553-9c3e-407f-b63c-a981c9597aee";
const decimalSeparator = ",";
const thousandsSeparator = " ";

test.describe("Html client", () => {
  test("Should format float number after input", async () => {
    await restoreAllDataTypesTable();
    await switchLanguageTo({locale: "cs-CZ", page: page});
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        allDataTypesLazyMenuItemsId
      ]);

    await waitForRowCountData(page, dataViewId,2099);

    await switchToFormPerspective({
      page: page,
      aPropertyId: currencyPropertyId
    });
    const numberEditor = page.locator(`#editor_${currencyPropertyId}`);
    await numberEditor.focus();

    await inputByPressingKeys({page: page, value: `-123456${decimalSeparator}789`})

    await waitForFocus({
      elementId: `editor_${currencyPropertyId}`,
      page: page
    })
    await page.keyboard.press("Tab");

    await expect(numberEditor).toHaveValue(`-123${thousandsSeparator}456${decimalSeparator}789`);

    await switchToTablePerspective({page: page});
  });
  test("Should reformat float number after changes", async () => {
    await putNumericTestDataToAllDataTypes();
    await switchLanguageTo({locale: "cs-CZ", page: page});
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        widgetsMenuItemId,
        allDataTypesLazyMenuItemsId
      ]);

    await waitForRowCountData(page, dataViewId,1);

    await switchToFormPerspective({
      page: page,
      aPropertyId: currencyPropertyId
    });
    const numberEditor = page.locator(`#editor_${currencyPropertyId}`);
    await numberEditor.evaluate(element => {
      element.focus();
      element.setSelectionRange(5, 5);
    });

    await page.keyboard.press("Backspace");

    const cursorPosition = await numberEditor.evaluate(element => element.selectionStart);
    expect(cursorPosition).toBe(4);

    await page.keyboard.press("Tab");

    await expect(numberEditor).toHaveValue(`12${thousandsSeparator}356${decimalSeparator}78`);
  });
});
