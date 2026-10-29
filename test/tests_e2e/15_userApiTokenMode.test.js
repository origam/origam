const { test, expect } = require("@playwright/test");
const { beforeEachTest, login, openMenuItem } = require('./testTools');
const { topMenuHeader, privateLinkReportMenuItemId } = require('./modelIds');
const { getStatusCode, getAccessToken } = require('./tools/userApiTools');

const privateLinkReportPath = "/api/private/link-report";

test.beforeEach(async ({ page }) => {
  await beforeEachTest(page);
  await expect(page.locator("#loginButton")).toBeVisible();
});

function testsForAuthenticationMode(authentication, name, tests) {
  const serverAuthentication =
    process.env.OpenIddictConfig__PrivateApiAuthentication || "Cookie";
  const describeWhenServerMatches =
    serverAuthentication === authentication ? test.describe : test.describe.skip;
  describeWhenServerMatches(name, tests);
}

testsForAuthenticationMode("Token", "User API in Token mode", () => {
  test("Should serve a public route to an anonymous user", async ({ page }) => {
    expect(await getStatusCode(page, "/api/public/invalid-json")).toBe(200);
  });

  test("Should reject an anonymous user on a private route", async ({ page }) => {
    expect(await getStatusCode(page, privateLinkReportPath)).toBe(401);
  });

  test("Should serve a private route to a user with a valid access token", async ({ page }) => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    expect(await getStatusCode(
      page,
      privateLinkReportPath,
      { Authorization: `Bearer ${accessToken}` }
    )).toBe(200);
  });

  test("Should reject an invalid access token on a private route", async ({ page }) => {
    expect(await getStatusCode(
      page,
      privateLinkReportPath,
      { Authorization: "Bearer invalid-token" }
    )).toBe(401);
  });

  test("Should reject a logged in user without a token on a private route outside of a frame", async ({ page }) => {
    await login(page);
    await expect(page.locator(`#${topMenuHeader}`)).toBeVisible();
    expect(await getStatusCode(page, privateLinkReportPath)).toBe(401);
  });

  test("Should open a private route web report from the menu of a logged in user and follow its link", async ({ page }) => {
    await login(page);
    const reportResponse = page.waitForResponse(
      response => response.url().includes(privateLinkReportPath)
    );
    await openMenuItem(page, [topMenuHeader, privateLinkReportMenuItemId]);
    expect((await reportResponse).status()).toBe(200);

    const reportFrame = (await reportResponse).request().frame();
    const reportLink = reportFrame.getByRole("link", { name: "TEST" });
    await reportLink.click();

    await expect(page.locator(
      "xpath=//*[normalize-space(@title)='All Data Types'][.//*[contains(@class, 'tabHandle')]]"
    )).toBeVisible();
  });
});
