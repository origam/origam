const { test, expect } = require("@playwright/test");
const { openMenuItem, login, beforeEachTest } = require('./testTools');
const { clearScreenConfiguration } = require("./dbTools");
const { topMenuHeader, reportWindowMenuItemId } = require("./modelIds");


let page;

test.beforeAll(async () => {
    await clearScreenConfiguration();
});

test.beforeEach(async ({ page: testPage }) => {
  page = testPage;
  await beforeEachTest(page);
});



test.describe("Html client", () => {
    test("Should open and close report window", async () => {
        await login(page);
        await openMenuItem(
            page,
            [
                topMenuHeader,
                reportWindowMenuItemId,
            ]);

        const reportWindowTab = page.locator("[class*='tabHandle']");
        await reportWindowTab.click();
        await expect(reportWindowTab).toHaveCount(0);
    });
});
