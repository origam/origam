const { beforeEachTest, afterEachTest, login, openMenuItem } = require('./testTools');
const { topMenuHeader, privateLinkReportMenuItemId } = require('./modelIds');
const { getStatusCode, getAccessToken } = require('./tools/userApiTools');

const privateLinkReportPath = "/api/private/link-report";

let browser;
let page;

beforeEach(async () => {
  [browser, page] = await beforeEachTest()
});

afterEach(async () => {
  await afterEachTest(browser);
  browser = undefined;
});

function testsForAuthenticationMode(authentication, name, tests) {
  const serverAuthentication =
    process.env.OpenIddictConfig__PrivateApiAuthentication || "Token";
  const describeWhenServerMatches =
    serverAuthentication === authentication ? describe : describe.skip;
  describeWhenServerMatches(name, tests);
}

testsForAuthenticationMode("Cookie", "User API in Cookie mode", () => {
  it("Should serve a public route to an anonymous user", async () => {
    expect(await getStatusCode(page, "/api/public/invalid-json")).toBe(200);
  });

  it("Should reject an anonymous user on a private route", async () => {
    expect(await getStatusCode(page, privateLinkReportPath)).toBe(401);
  });

  it("Should reject a valid access token on a private route", async () => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    expect(await getStatusCode(
      page,
      privateLinkReportPath,
      { Authorization: `Bearer ${accessToken}` }
    )).toBe(401);
  });

  it("Should reject an invalid access token on a private route", async () => {
    expect(await getStatusCode(
      page,
      privateLinkReportPath,
      { Authorization: "Bearer invalid-token" }
    )).toBe(401);
  });

  it("Should serve a private route to a logged in user without a token", async () => {
    await login(page);
    await page.waitForXPath(`//*[@id='${topMenuHeader}']`, { visible: true });
    expect(await getStatusCode(page, privateLinkReportPath)).toBe(200);
  });

  it("Should open a private route web report from the menu of a logged in user and follow its link", async () => {
    await login(page);
    const reportResponse = page.waitForResponse(
      response => response.url().includes(privateLinkReportPath)
    );
    await openMenuItem(page, [topMenuHeader, privateLinkReportMenuItemId]);
    expect((await reportResponse).status()).toBe(200);

    const reportFrame = (await reportResponse).frame();
    const reportLink = await reportFrame.waitForXPath(
      `//a[contains(., 'TEST')]`,
      { visible: true }
    );
    await reportLink.click();

    await page.waitForXPath(
      `//*[normalize-space(@title)='All Data Types'][.//*[contains(@class, 'tabHandle')]]`,
      { visible: true }
    );
  });
});
