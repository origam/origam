const { beforeEachTest, afterEachTest, login } = require('./testTools');
const { topMenuHeader } = require('./modelIds');
const { getStatusCode, getAccessToken } = require('./tools/userApiTools');

const soapServicePaths = ["/soap/DataService", "/soap/WorkflowService"];
const allDataTypesDataStructureId = "31791c3b-7265-439e-ac96-ddd57aa82579";
const loadDataRequest =
  '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"' +
  ' xmlns:a="http://asapenginewebapi.advantages.cz/"><s:Body><a:LoadData>' +
  `<a:dataStructureId>${allDataTypesDataStructureId}</a:dataStructureId>` +
  "</a:LoadData></s:Body></s:Envelope>";

let browser;
let page;

beforeEach(async () => {
  [browser, page] = await beforeEachTest()
});

afterEach(async () => {
  await afterEachTest(browser);
  browser = undefined;
});

function testsForAuthenticationMethod(authenticationMethod, name, tests) {
  const serverAuthenticationMethod =
    process.env.SoapAPI__AuthenticationMethod || "CookieAndToken";
  const describeWhenServerMatches =
    process.env.SoapAPI__Enabled === "true" &&
    serverAuthenticationMethod === authenticationMethod
      ? describe
      : describe.skip;
  describeWhenServerMatches(name, tests);
}

async function expectStatusOnSoapServices(expectedStatus, headers = {}) {
  for (const soapServicePath of soapServicePaths) {
    expect(await getStatusCode(page, soapServicePath, headers)).toBe(expectedStatus);
  }
}

async function loadDataOverSoap(headers = {}) {
  return await page.evaluate(
    async (requestBody, requestHeaders) => {
      const response = await fetch("/soap/DataService", {
        method: "POST",
        headers: { "Content-Type": "application/xml", ...requestHeaders },
        body: requestBody,
      });
      return { status: response.status, body: await response.text() };
    },
    loadDataRequest,
    headers
  );
}

async function loginAndWaitForMenu() {
  await login(page);
  await page.waitForXPath(`//*[@id='${topMenuHeader}']`, { visible: true });
}

testsForAuthenticationMethod("Token", "SOAP API in Token mode", () => {
  it("Should reject an anonymous user", async () => {
    await expectStatusOnSoapServices(401);
  });

  it("Should serve a user with a valid access token", async () => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    await expectStatusOnSoapServices(200, { Authorization: `Bearer ${accessToken}` });
  });

  it("Should reject an invalid access token", async () => {
    await expectStatusOnSoapServices(401, { Authorization: "Bearer invalid-access-token" });
  });

  it("Should reject a logged in user without an access token", async () => {
    await loginAndWaitForMenu();
    await expectStatusOnSoapServices(401);
  });

  it("Should load data for a user with a valid access token", async () => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    const response = await loadDataOverSoap({ Authorization: `Bearer ${accessToken}` });
    expect(response.status).toBe(200);
    expect(response.body).toContain("LoadDataResponse");
  });

  it("Should not load data for an anonymous user", async () => {
    const response = await loadDataOverSoap();
    expect(response.status).toBe(401);
    expect(response.body).not.toContain("LoadDataResponse");
  });
});

testsForAuthenticationMethod("Cookie", "SOAP API in Cookie mode", () => {
  it("Should reject an anonymous user", async () => {
    await expectStatusOnSoapServices(401);
  });

  it("Should serve a logged in user", async () => {
    await loginAndWaitForMenu();
    await expectStatusOnSoapServices(200);
  });

  it("Should reject a user with only a valid access token", async () => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    await expectStatusOnSoapServices(401, { Authorization: `Bearer ${accessToken}` });
  });

  it("Should load data for a logged in user", async () => {
    await loginAndWaitForMenu();
    const response = await loadDataOverSoap();
    expect(response.status).toBe(200);
    expect(response.body).toContain("LoadDataResponse");
  });
});

testsForAuthenticationMethod("CookieAndToken", "SOAP API in CookieAndToken mode", () => {
  it("Should reject an anonymous user", async () => {
    await expectStatusOnSoapServices(401);
  });

  it("Should serve a user with a valid access token", async () => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    await expectStatusOnSoapServices(200, { Authorization: `Bearer ${accessToken}` });
  });

  it("Should reject an invalid access token", async () => {
    await expectStatusOnSoapServices(401, { Authorization: "Bearer invalid-access-token" });
  });

  it("Should serve a logged in user without an access token", async () => {
    await loginAndWaitForMenu();
    await expectStatusOnSoapServices(200);
  });

  it("Should load data for a user with a valid access token", async () => {
    const accessToken = await getAccessToken(page);
    expect(accessToken).toBeTruthy();
    const response = await loadDataOverSoap({ Authorization: `Bearer ${accessToken}` });
    expect(response.status).toBe(200);
    expect(response.body).toContain("LoadDataResponse");
  });

  it("Should load data for a logged in user", async () => {
    await loginAndWaitForMenu();
    const response = await loadDataOverSoap();
    expect(response.status).toBe(200);
    expect(response.body).toContain("LoadDataResponse");
  });

  it("Should not load data for an anonymous user", async () => {
    const response = await loadDataOverSoap();
    expect(response.status).toBe(401);
    expect(response.body).not.toContain("LoadDataResponse");
  });
});
