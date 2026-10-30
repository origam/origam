const { test, expect } = require("@playwright/test");
const { beforeEachTest, login } = require("./testTools");
const { getStatusCode, getAccessToken } = require("./tools/userApiTools");

const cookieEnabledPath = "/api/private/allDataTypesCookie";
const tokenOnlyPath = "/api/private/link-report";

test.describe("User API with additional cookie authentication", () => {
  test.skip(
    process.env.OpenIddictConfig__PrivateApiAuthentication === "Cookie",
    "These tests require token mode with cookie authentication enabled for the test endpoint."
  );

  test.beforeEach(async ({ page }) => {
    await beforeEachTest(page);
    await expect(page.locator("#loginButton")).toBeVisible();
  });

  test("Cookie-enabled endpoint accepts a cookie", async ({ page }) => {
    await login(page);
    expect(await getStatusCode(page, cookieEnabledPath)).toBe(200);
  });

  test("Cookie-enabled endpoint accepts a token", async ({ page }) => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    expect(await getStatusCode(page, cookieEnabledPath, {
      Authorization: `Bearer ${accessToken}`,
    })).toBe(200);
  });

  test("Cookie-enabled endpoint rejects an unauthenticated request", async ({ page }) => {
    expect(await getStatusCode(page, cookieEnabledPath)).toBe(401);
  });

  test("Token-only endpoint rejects a cookie", async ({ page }) => {
    await login(page);
    expect(await getStatusCode(page, tokenOnlyPath)).toBe(401);
  });

  test("Token-only endpoint accepts a token", async ({ page }) => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    expect(await getStatusCode(page, tokenOnlyPath, {
      Authorization: `Bearer ${accessToken}`,
    })).toBe(200);
  });

  test("Token-only endpoint rejects an unauthenticated request", async ({ page }) => {
    expect(await getStatusCode(page, tokenOnlyPath)).toBe(401);
  });
});
