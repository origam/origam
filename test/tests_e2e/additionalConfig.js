const userName = process.env.ORIGAM_E2E_USERNAME || "washi";
const password = process.env.ORIGAM_E2E_PASSWORD || "blabla";
const backEndUrl = process.env.ORIGAM_E2E_URL || "https://localhost";

module.exports = {userName, password, backEndUrl};
