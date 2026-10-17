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

import { IApplication } from "model/entities/types/IApplication";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";

export interface IApplicationWorkState {
  ready: boolean;
  reasons: string[];
}

function addScreenWorkReasons(reasons: string[], openedScreen: IOpenedScreen) {
  const screenId = `${openedScreen.menuItemId}:${openedScreen.order}`;
  if (!openedScreen.content) {
    return;
  }
  if (openedScreen.content.isLoading || !openedScreen.content.formScreen) {
    reasons.push(`screen:${screenId}:loading`);
    return;
  }

  const formScreen = openedScreen.content.formScreen;
  if (formScreen.formScreenLifecycle.isWorking) {
    reasons.push(`screen:${screenId}:working`);
  }
  if (!formScreen.formScreenLifecycle.allDataViewsSteady) {
    reasons.push(`screen:${screenId}:settling`);
  }
  for (const dataView of formScreen.dataViews) {
    if (dataView.isWorking) {
      reasons.push(`data-view:${dataView.modelInstanceId}:working`);
    }
  }
}

export function getApplicationWorkState(application: IApplication): IApplicationWorkState {
  const reasons: string[] = [];
  if (application.applicationLifecycle?.isWorking) {
    reasons.push("application:working");
  }
  if (!application.workbench) {
    reasons.push("workbench:loading");
  } else {
    if (application.workbench.mainMenuEnvelope.isLoading) {
      reasons.push("main-menu:loading");
    }
    const openedScreens = [
      ...application.workbench.openedScreens.items,
      ...application.workbench.openedDialogScreens.items,
    ];
    openedScreens.forEach(openedScreen => addScreenWorkReasons(reasons, openedScreen));
  }
  return { ready: reasons.length === 0, reasons };
}

export function installApplicationDebugMonitor(application: IApplication) {
  if (!window.localStorage.getItem("debugApplicationState")) {
    return;
  }
  (window as any).origamDebug = {
    getApplicationWorkState: () => getApplicationWorkState(application),
  };
}
