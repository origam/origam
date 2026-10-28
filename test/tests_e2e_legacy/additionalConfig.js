require("dotenv").config({ path: require("path").join(__dirname, ".env"), quiet: true });
const userName = process.env.ORIGAM_E2E_USERNAME || "washi";
const password = process.env.ORIGAM_E2E_PASSWORD || "blabla";
const backEndUrl = process.env.ORIGAM_E2E_URL || "https://localhost";
const serverClientSecret = process.env.ORIGAM_E2E_SERVER_CLIENT_SECRET || "serverSecret";

module.exports = {userName, password, backEndUrl, serverClientSecret};
