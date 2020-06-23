import "mobx-react-lite/batchingForReactDom";

import axios from "axios";
import { flow } from "mobx";
import { getApi } from "model/selectors/getApi";
import { ensureLogin, userManager } from "oauth";
import React from "react";
import ReactDOM from "react-dom";
import "react-tippy/dist/tippy.css";
import { Root } from "Root";
import "./index.scss";
import { createApplication } from "./model/factories/createApplication";
import "./rootContainer";
import * as serviceWorker from "./serviceWorker";
import Cookie from "js-cookie";

if (process.env.REACT_APP_SELENIUM_KICK) {
  axios.post("http://127.0.0.1:3500/app-reload");
}

if (process.env.NODE_ENV === "development") {
  axios.defaults.timeout = 3600000;
  (window as any).ORIGAM_CLIENT_AXIOS_LIB = axios;
}

(window as any).ORIGAM_CLIENT_REVISION_HASH = process.env.REACT_APP_GIT_REVISION_HASH || "UNKNOWN";
(window as any).ORIGAM_CLIENT_REVISION_DATE = process.env.REACT_APP_GIT_REVISION_DATE || "UNKNOWN";

async function main() {
  const locationHash = window.location.hash;
  const TOKEN_OVR_HASH = "#origamAuthTokenOverride=";
  if (locationHash.startsWith(TOKEN_OVR_HASH)) {
    console.log("Set auth token to:", locationHash.replace(TOKEN_OVR_HASH, ""));
    sessionStorage.setItem("origamAuthTokenOverride", locationHash.replace(TOKEN_OVR_HASH, ""));
    window.location.hash = "";
  }
  const BACKEND_OVR_HASH = "#origamBackendOverride=";
  if (locationHash.startsWith(BACKEND_OVR_HASH)) {
    const backendUrl = locationHash.replace(BACKEND_OVR_HASH, "");
    const newUrl = backendUrl + `#origamBackendOverrideReturn=${window.location.origin}`;
    // debugger;
    Cookie.set('backendUrl', backendUrl);
    window.location.assign(newUrl);
    return;
  }

  const BACKEND_OVR_RETURN_HASH = "#origamBackendOverrideReturn=";
  if (locationHash.startsWith(BACKEND_OVR_RETURN_HASH)) {
    window.sessionStorage.setItem(
      "teleportAfterLogin",
      locationHash.replace(BACKEND_OVR_RETURN_HASH, "")
    );
    window.location.hash = "";
    for (let k of Object.keys(window.sessionStorage)) {
      if (k.startsWith("oidc.user")) {
        window.sessionStorage.removeItem(k);
      }
    }
  }
  // debugger;
  const user = await ensureLogin();
  if (user) {
    if (window.sessionStorage.getItem("teleportAfterLogin")) {
      const newUrl =
        window.sessionStorage.getItem("teleportAfterLogin") +
        `#origamAuthTokenOverride=${user.access_token}`;
      window.sessionStorage.removeItem("teleportAfterLogin");
      //window.location.assign(newUrl);
      return;
    }
    const application = createApplication();
    getApi(application).setAccessToken(user.access_token);
    sessionStorage.setItem("origamAuthToken", user.access_token);
    userManager.events.addUserLoaded((user) => {
      getApi(application).setAccessToken(user.access_token);
      sessionStorage.setItem("origamAuthToken", user.access_token);
    });
    flow(application.run.bind(application))();

    ReactDOM.render(<Root application={application} />, document.getElementById("root"));
  } else {
    // TODO: ???
  }
}

main();

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
