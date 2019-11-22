import { CMain } from "gui02/connections/CMain";
import { flow } from "mobx";
import { Provider } from "mobx-react";
import React from "react";
import ReactDOM from "react-dom";
import 'react-tippy/dist/tippy.css';
import "./index.scss";
import { createApplication } from "./model/factories/createApplication";
import * as serviceWorker from "./serviceWorker";

const application = createApplication();
flow(application.run.bind(application))();

ReactDOM.render(
  <Provider application={application}>
    {/*<Main />*/}
    <CMain />
  </Provider>,
  document.getElementById("root")
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
  

