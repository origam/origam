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

        await sleep(2500);
        let reportWindowTab = await page.$(`[class*='tabHandle']`);
        const box = await reportWindowTab.boundingBox();
        await page.mouse.click(
          box.x + 1,
          box.y + 1
        );

        await sleep(1000);
        reportWindowTab = await page.$(`[class*='tabHandle']`);
        await sleep(500);
        expect(reportWindowTab).toBeNull();
    });
});