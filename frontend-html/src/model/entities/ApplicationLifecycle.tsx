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

import { action, computed, observable } from "mobx";
import { createWorkbench } from "../factories/createWorkbench";
import { getApi } from "../selectors/getApi";
import { getApplication } from "../selectors/getApplication";
import { IApplicationLifecycle } from "./types/IApplicationLifecycle";
import { stopWorkQueues } from "model/actions/WorkQueues/stopWorkQueues";
import { stopAllFormsAutorefresh } from "model/actions/Workbench/stopAllFormsAutorefresh";
import { userManager } from "oauth";
import { getWorkbench } from "model/selectors/getWorkbench";
import { getOpenedScreens } from "model/selectors/getOpenedScreens";
import { QuestionLogoutWithDirtyData } from "gui/Components/Dialogs/QuestionLogoutWithDirtyData";
import React from "react";
import { getDialogStack } from "model/selectors/DialogStack/getDialogStack";

export class ApplicationLifecycle implements IApplicationLifecycle {
  $type_IApplicationLifecycle: 1 = 1;

  @observable loginPageMessage?: string | undefined;

  @observable inFlow = 0;

  @computed get isWorking() {
    return this.inFlow > 0;
  }

  *onLoginFormSubmit(args: { event: any; userName: string; password: string }) {
    try {
      this.inFlow++;
      args.event.preventDefault();
      yield*this.performLogin(args);
    } finally {
      this.inFlow--;
    }
  }

  *onSignOutClick(args: { event: any }) {
    yield*this.requestSignout();
  }

  *requestSignout(): any {
    const workbench = getWorkbench(this);
    const openedScreens = workbench && getOpenedScreens(workbench);
    let isSomeDirtyScreen = false;
    for (let openedScreen of openedScreens.items) {
      isSomeDirtyScreen = !!(isSomeDirtyScreen || openedScreen.content?.formScreen?.isDirty);
    }
    if (isSomeDirtyScreen) {
      const isReallySignOut = yield new Promise((resolve: (answer: boolean) => void) => {
        const closeDialog = getDialogStack(this).pushDialog(
          "",
          <QuestionLogoutWithDirtyData
            onYesClick={() => {
              closeDialog();
              resolve(true);
            }}
            onNoClick={() => {
              closeDialog();
              resolve(false);
            }}
          />
        );
      });

      if (!isReallySignOut) return;
    }

    yield*this.performLogout();
  }

  *run() {
    yield*this.reuseAuthToken();
  }

  *performLogin(args: { userName: string; password: string }): any {
    try {
      const api = getApi(this);
      const token = yield api.login({
        UserName: args.userName,
        Password: args.password,
      });
      yield*this.anounceAuthToken(token);
    } catch (error) {
      // TODO: Distinguish between connection error and bad credentials etc.
      this.setLoginPageMessage("Login failed.");
      throw error;
    }
  }

  *performLogout() {
    const api = getApi(this);
    const application = getApplication(this);
    window.sessionStorage.removeItem("origamAuthToken");
    for (let sessionStorageKey of Object.keys(window.sessionStorage)) {
      if (sessionStorageKey.startsWith("oidc.user")) {
        // That is an oauth session...
        api.resetAccessToken();
        yield userManager.signoutRedirect();
        return;
      }
    }
    yield*stopAllFormsAutorefresh(application.workbench!)();
    yield*stopWorkQueues(application.workbench!)();
    application.resetWorkbench();
    try {
      yield api.logout();
    } finally {
      api.resetAccessToken();
    }
    return null;
  }

  *reuseAuthToken() {
    const token = window.sessionStorage.getItem("origamAuthToken");
    if (token) {
      yield*this.anounceAuthToken(token);
    }
  }

  *anounceAuthToken(token: string) {
    const api = getApi(this);
    const application = getApplication(this);
    window.sessionStorage.setItem("origamAuthToken", token);
    api.setAccessToken(token);
    const workbench = createWorkbench();
    application.setWorkbench(workbench);
    yield*workbench.run();
  }

  @action.bound
  setLoginPageMessage(msg: string): void {
    this.loginPageMessage = msg;
  }

  @action.bound
  resetLoginPageMessage(): void {
    this.loginPageMessage = undefined;
  }

  parent?: any;
}
