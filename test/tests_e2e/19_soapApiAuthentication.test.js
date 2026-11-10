const { test, expect } = require("@playwright/test");
const { beforeEachTest, login } = require("./testTools");
const { getStatusCode, getAccessToken } = require("./tools/userApiTools");

const soapServicePaths = ["/soap/DataService", "/soap/WorkflowService"];

test.beforeEach(async ({ page }) => {
  await beforeEachTest(page);
  await expect(page.locator("#loginButton")).toBeVisible();
});

function testsForAuthenticationMethod(authenticationMethod, name, tests) {
  const serverAuthenticationMethod =
    process.env.SoapAPI__AuthenticationMethod || "Cookie";
  const describeWhenServerMatches =
    process.env.SoapAPI__Enabled === "true" &&
    serverAuthenticationMethod === authenticationMethod
      ? test.describe
      : test.describe.skip;
  describeWhenServerMatches(name, tests);
}

async function expectStatusOnSoapServices(page, expectedStatus, headers = {}) {
  for (const soapServicePath of soapServicePaths) {
    expect(await getStatusCode(page, soapServicePath, headers)).toBe(expectedStatus);
  }
}

testsForAuthenticationMethod("Token", "SOAP API in Token mode", () => {
  test("Should reject an anonymous user", async ({ page }) => {
    await expectStatusOnSoapServices(page, 401);
  });

  test("Should serve a user with a valid access token", async ({ page }) => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    await expectStatusOnSoapServices(page, 200, {
      Authorization: `Bearer ${accessToken}`,
    });
  });

  test("Should reject an invalid access token", async ({ page }) => {
    await expectStatusOnSoapServices(page, 401, {
      Authorization: "Bearer invalid-access-token",
    });
  });

  test("Should reject a logged in user without an access token", async ({ page }) => {
    await login(page);
    await expectStatusOnSoapServices(page, 401);
  });
});

testsForAuthenticationMethod("Cookie", "SOAP API in Cookie mode", () => {
  test("Should reject an anonymous user", async ({ page }) => {
    await expectStatusOnSoapServices(page, 401);
  });

  test("Should serve a logged in user", async ({ page }) => {
    await login(page);
    await expectStatusOnSoapServices(page, 200);
  });

  test("Should reject a user with only a valid access token", async ({ page }) => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    await expectStatusOnSoapServices(page, 401, {
      Authorization: `Bearer ${accessToken}`,
    });
  });
});
