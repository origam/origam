import React from "react";
import ReactDOM from "react-dom";
import "./index.css";
import * as serviceWorker from "./serviceWorker";

import { WorkbenchPage } from "./gui/Workbench/WorkbenchPage";
import { LoginPage } from "./gui/Login/LoginPage";
import { action, observable, runInAction} from "mobx";
import { ApplicationLifecycle } from "./model/ApplicationLifecycle";
import { Application } from "./model/Application";
import { Provider } from "mobx-react";
import { Main } from "./gui/Main";
import { createApplication } from "./model/factories/createApplication";

const application = createApplication();
application.run();

ReactDOM.render(
  <Provider application={application}>
    <Main />
  </Provider>,
  document.getElementById("root")
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();



