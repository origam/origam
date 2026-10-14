const { test, expect } = require("@playwright/test");
const { openMenuItem, login, beforeEachTest, switchToFormPerspective,
  waitForFocus } = require('./testTools');
const {
  topMenuHeader
} = require("./modelIds");

let page;

test.beforeEach(async ({ page: testPage }) => {
  page = testPage;
  await beforeEachTest(page);
});



async function waitForErrorMessage(messageBegging) {
  const message = page.locator("div[class*='dialogMessage']");
  await expect.poll(async () =>
    (await message.innerText()).toLocaleLowerCase()
      .includes(messageBegging.toLocaleLowerCase())
  ).toBe(true);
}

const stateTransitionMenuId = "menu_f69ef86f-7fad-44c0-9ecb-78164a656a38";
const statePropertyId = "908c313c-fc0e-44d6-8c1a-c4c1659f08fd";

test.describe("Html client", () => {
  test("Should show an error message after invalid state change", async () => {
    await login(page);
    await openMenuItem(
      page,
      [
        topMenuHeader,
        stateTransitionMenuId
      ]);

    await switchToFormPerspective({
      page: page,
      aPropertyId: statePropertyId
    });
    const stateEditor = page.locator(`#editor_${statePropertyId}`);
    await stateEditor.fill("");

    await stateEditor.pressSequentially("end");
    await expect(stateEditor).toHaveValue("end");

    await waitForFocus({
      elementId: `editor_${statePropertyId}`,
      page: page
    });
    await page.keyboard.press("Tab");

    await expect(page.locator("#saveButton .isRed")).toBeVisible();
    const saveResponsePromise = page.waitForResponse(response =>
      response.url().includes("/internalApi/UIService/SaveData/")
    );
    await page.locator("#saveButton").click();
    const saveResponse = await saveResponsePromise;
    const errorResponse = await saveResponse.json();

    const expectedMessage = Object.hasOwn(errorResponse, "detail")
      ? "State of 'State Transition' cannot enter the state 'Error'. Current state: 'end'"
      : "An error has occured. There may be some details in the log file.";
    await waitForErrorMessage(expectedMessage);
  });
});
