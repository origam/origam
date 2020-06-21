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
  }
  const user = await ensureLogin();
  if (user) {
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
