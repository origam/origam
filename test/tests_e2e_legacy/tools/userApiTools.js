const { userName, password, serverClientSecret } = require("../additionalConfig");

async function getStatusCode(page, path, headers = {}) {
  return await page.evaluate(
    async (requestPath, requestHeaders) =>
      (await fetch(requestPath, { headers: requestHeaders })).status,
    path,
    headers
  );
}

async function getAccessToken(page) {
  return await page.evaluate(
    async tokenRequest => {
      const response = await fetch("/connect/token", {
        method: "POST",
        body: new URLSearchParams(tokenRequest),
      });
      return (await response.json()).access_token;
    },
    {
      grant_type: "password",
      client_id: "serverClient",
      client_secret: serverClientSecret,
      scope: "internal_api",
      username: userName,
      password: password,
    }
  );
}

module.exports = { getStatusCode, getAccessToken };
