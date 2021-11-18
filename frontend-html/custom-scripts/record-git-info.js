const dotenv = require("dotenv");
const fs = require("fs");

const ENV_FILE_NAME = ".env.production.local";

if (fs.existsSync(ENV_FILE_NAME)) {
  throw new Error(
    `File '${ENV_FILE_NAME}' already exists and would be overwritten by git commit info.`
  );
}

const { execSync } = require("child_process");
try {
  const REACT_APP_GIT_REVISION_HASH = execSync("git rev-parse HEAD")
    .toString()
    .trim();
  const REACT_APP_GIT_REVISION_DATE = execSync("git log -1 --format=%cd")
    .toString()
    .trim();

  fs.writeFileSync(
    ENV_FILE_NAME,
    `
REACT_APP_GIT_REVISION_HASH=${REACT_APP_GIT_REVISION_HASH}
REACT_APP_GIT_REVISION_DATE=${REACT_APP_GIT_REVISION_DATE}
  `
  );
} catch (e) {
  console.error("Cannot determine git revision:", e);
}
