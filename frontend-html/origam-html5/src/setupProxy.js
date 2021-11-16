const createProxyMiddleware = require("http-proxy-middleware");

const PROXY_PATH_PATTERNS = [
  "/internalApi/*",
  "/customAssets/*",
  "/chatrooms/*",
  "/api/*",
  "/connect/*",
  "/assets/*",
  // "/home/*",
  // "/icons/*",
  // "/lib/*",
  // "/js/*",
  // "/css/*",
  "/Account/*",
  "/account/*",
  "/.well-known/*",
];

module.exports = function (app) {
  const proxyTarget = makeProxyTarget();
  const customProxy = makeCustomProxy(proxyTarget);
  for(let pattern of PROXY_PATH_PATTERNS) {
    app.use(pattern, createProxyMiddleware(customProxy));
  }
};

function makeProxyTarget() {
  if (process.env.WDS_PROXY_TARGET) return process.env.WDS_PROXY_TARGET;
  else return "https://localhost:5001";
}

const proxyCommon = {
  //logLevel: 'debug',
  changeOrigin: process.env.WDS_CHANGE_ORIGIN || false,
};

function makeCustomProxy(proxyTarget) {
  return {
    target: proxyTarget,
    secure: false,
    ...proxyCommon,
  };
}
