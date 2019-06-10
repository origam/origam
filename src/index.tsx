import React from "react";
import ReactDOM from "react-dom";
// import App from "./presenter/react/App";
import * as serviceWorker from "./serviceWorker";
import "./presenter/react/styles/main.scss";
import "react-tippy/dist/tippy.css";

import { ApplicationScope } from "./factory/ApplicationScope";
import { Application } from "./presenter/react/Application";
import { Provider } from "mobx-react";
import { IApplicationScope } from "./factory/types/IApplicationScope";

const applicationScope: IApplicationScope = new ApplicationScope();
applicationScope.aStartTheMiracle.do();

ReactDOM.render(
  <Provider
    applicationScope={applicationScope}
    mainViewsScope={applicationScope.mainViewsScope}
    loggedUserScope={applicationScope.loggedUserScope}
    mainMenuScope={applicationScope.mainMenuScope}
  >
    <Application />
  </Provider>,
  document.getElementById("root")
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
