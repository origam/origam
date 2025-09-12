/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import "mobx-react-lite/batchingForReactDom";
import React from "react";
import ReactDOM from "react-dom";
import {
  HashRouter as Router,
  Route,
  Switch,
  Redirect,
} from "react-router-dom";
import "./admin.scss";
import { ChatApp } from "./ChatApp/componentIntegrations/ChatApp";
import "./index.scss";
import "./spinner.scss";
import * as serviceWorker from "./serviceWorker";
import { translationsInit } from "util/translation";
import moment from "moment";
import "moment/min/locales";
import { getLocaleFromCookie } from "util/cookies";

function Routed() {
  return (
    <Router>
      <Switch>
        <Route path="/" exact={true}>
          <Redirect to="/chatroom" />
        </Route>
        <Route path="/chatroom">
          <ChatApp />
        </Route>
      </Switch>
    </Router>
  );
}

async function main() {
  const locale = getLocaleFromCookie();
  moment.locale(locale);
  try {
    await translationsInit();
  } catch (e) {
    console.error("Could not initialize translations.");
    console.error(e);
  }

  ReactDOM.render(
    <React.StrictMode>
      <Routed />
      {/*<App3 />*/}
    </React.StrictMode>,
    document.getElementById("root")
  );
}

main();

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();


