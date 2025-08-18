const {
    sleep, openMenuItem, login, afterEachTest, beforeEachTest
} = require('./testTools');
const { clearScreenConfiguration } = require("./dbTools");
const { topMenuHeader, reportWindowMenuItemId } = require("./modelIds");


let browser;
let page;

beforeAll(async () => {
    await clearScreenConfiguration();
});

beforeEach(async () => {
    [browser, page] = await beforeEachTest()
});

afterEach(async () => {
    await afterEachTest(browser);
    browser = undefined;
});

describe("Html client", () => {
    it("Should open and close report window", async () => {
        await login(page);
        await openMenuItem(
            page,
            [
                topMenuHeader,
                reportWindowMenuItemId,
            ]);

        let reportWindowTab = await page.waitForXPath(`//*[contains(@class, 'tabHandle')]`, { visible: true });
        await sleep(500);

        await reportWindowTab.click();
        
        reportWindowTab = await page.$(`[class*='tabHandle']`);
        expect(reportWindowTab).toBeNull();
    });
});