const { beforeEachTest, afterEachTest } = require('./testTools');

let browser;
let page;

beforeEach(async () => {
  [browser, page] = await beforeEachTest()
});

afterEach(async () => {
  await afterEachTest(browser);
  browser = undefined;
});

async function getStatusCode(path) {
  return await page.evaluate(
    async requestPath => (await fetch(requestPath)).status,
    path
  );
}

describe("User API", () => {
  it("Should serve a public route to an anonymous user", async () => {
    expect(await getStatusCode("/api/public/invalid-json")).toBe(200);
  });

  it("Should reject an anonymous user on a private route", async () => {
    expect(await getStatusCode("/api/private/link-report")).toBe(401);
  });
});
