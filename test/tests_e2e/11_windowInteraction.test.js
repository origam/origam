const {
    sleep, xPathContainsClass, openMenuItem, login, catchRequests, afterEachTest, beforeEachTest
} = require('./testTools');
const { restoreWidgetSectionTestMaster, clearScreenConfiguration } = require("./dbTools");
const { widgetsMenuItemId, sectionsMenuItemId, masterDetailMenuItemId,
    topMenuHeader, masterDataViewId, detailDataViewId, detailEditorId, detailTabHandelId } = require("./modelIds");


let browser;
let page;
const reportWindowId = "menu_3dc51183-a78e-4663-bd41-97b230de7b7d";

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
                reportWindowId,
            ]);

        await sleep(1500);
        let reportWindowTab = await page.$(`[class*='tabHandle']`);
        const box = await reportWindowTab.boundingBox();
        await page.mouse.click(
          box.x + 1,
          box.y + 1
        );

        await sleep(500);
        reportWindowTab = await page.$(`[class*='tabHandle']`);
        await sleep(500);
        expect(reportWindowTab).toBe(null);
    });
});