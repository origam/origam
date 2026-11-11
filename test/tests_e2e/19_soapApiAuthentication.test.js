const { test, expect } = require("@playwright/test");
const { beforeEachTest, login } = require("./testTools");
const { getStatusCode, getAccessToken } = require("./tools/userApiTools");

const soapServicePaths = ["/soap/DataService", "/soap/WorkflowService"];
const allDataTypesDataStructureId = "31791c3b-7265-439e-ac96-ddd57aa82579";
const loadDataRequest =
  '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"' +
  ' xmlns:a="http://asapenginewebapi.advantages.cz/"><s:Body><a:LoadData>' +
  `<a:dataStructureId>${allDataTypesDataStructureId}</a:dataStructureId>` +
  "</a:LoadData></s:Body></s:Envelope>";

test.beforeEach(async ({ page }) => {
  await beforeEachTest(page);
  await expect(page.locator("#loginButton")).toBeVisible();
});

function testsForAuthenticationMethod(authenticationMethod, name, tests) {
  const serverAuthenticationMethod =
    process.env.SoapAPI__AuthenticationMethod || "CookieAndToken";
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

async function loadDataOverSoap(page, headers = {}) {
  return await page.evaluate(
    async ({ requestBody, requestHeaders }) => {
      const response = await fetch("/soap/DataService", {
        method: "POST",
        headers: { "Content-Type": "application/xml", ...requestHeaders },
        body: requestBody,
      });
      return { status: response.status, body: await response.text() };
    },
    { requestBody: loadDataRequest, requestHeaders: headers }
  );
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

  test("Should load data for a user with a valid access token", async ({ page }) => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    const response = await loadDataOverSoap(page, {
      Authorization: `Bearer ${accessToken}`,
    });
    expect(response.status).toBe(200);
    expect(response.body).toContain("LoadDataResponse");
  });

  test("Should not load data for an anonymous user", async ({ page }) => {
    const response = await loadDataOverSoap(page);
    expect(response.status).toBe(401);
    expect(response.body).not.toContain("LoadDataResponse");
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

  test("Should load data for a logged in user", async ({ page }) => {
    await login(page);
    const response = await loadDataOverSoap(page);
    expect(response.status).toBe(200);
    expect(response.body).toContain("LoadDataResponse");
  });
});

testsForAuthenticationMethod("CookieAndToken", "SOAP API in CookieAndToken mode", () => {
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

  test("Should serve a logged in user without an access token", async ({ page }) => {
    await login(page);
    await expectStatusOnSoapServices(page, 200);
  });

  test("Should load data for a user with a valid access token", async ({ page }) => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    const response = await loadDataOverSoap(page, {
      Authorization: `Bearer ${accessToken}`,
    });
    expect(response.status).toBe(200);
    expect(response.body).toContain("LoadDataResponse");
  });

  test("Should load data for a logged in user", async ({ page }) => {
    await login(page);
    const response = await loadDataOverSoap(page);
    expect(response.status).toBe(200);
    expect(response.body).toContain("LoadDataResponse");
  });

  test("Should not load data for an anonymous user", async ({ page }) => {
    const response = await loadDataOverSoap(page);
    expect(response.status).toBe(401);
    expect(response.body).not.toContain("LoadDataResponse");
  });
});
