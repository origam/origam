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

import { installApplicationDebugMonitor } from "utils/ApplicationDebugMonitor";

function applicationWithScreen(args: {
  screenWorking?: boolean;
  steady?: boolean;
  dataViewWorking?: boolean;
}) {
  const formScreen = {
    formScreenLifecycle: {
      isWorking: args.screenWorking ?? false,
      allDataViewsSteady: args.steady ?? true,
    },
    dataViews: [{ modelInstanceId: "orders", isWorking: args.dataViewWorking ?? false }],
  };
  return {
    workbench: {
      mainMenuEnvelope: { isLoading: false },
      openedScreens: {
        items: [{ menuItemId: "sales", order: 1, content: { isLoading: false, formScreen } }],
      },
      openedDialogScreens: { items: [] },
    },
  } as any;
}

describe("application debug monitor", () => {
  beforeEach(() => {
    localStorage.setItem("debugApplicationState", "1");
    delete (window as any).origamDebug;
  });

  afterEach(() => {
    localStorage.removeItem("debugApplicationState");
    delete (window as any).origamDebug;
  });

  test("exposes why application work has not settled", () => {
    installApplicationDebugMonitor(applicationWithScreen({
      screenWorking: true,
      steady: false,
      dataViewWorking: true,
    }));

    expect((window as any).origamDebug.getApplicationWorkState()).toEqual({
      ready: false,
      reasons: [
        "screen:sales:1:working",
        "screen:sales:1:settling",
        "data-view:orders:working",
      ],
    });
  });

  test("reports ready when tracked application work has settled", () => {
    installApplicationDebugMonitor(applicationWithScreen({}));

    expect((window as any).origamDebug.getApplicationWorkState()).toEqual({
      ready: true,
      reasons: [],
    });
  });

  test("ignores report windows without form content", () => {
    const application = applicationWithScreen({});
    application.workbench.openedDialogScreens.items.push({
      menuItemId: "report",
      order: 1,
      content: null,
    });
    installApplicationDebugMonitor(application);

    expect((window as any).origamDebug.getApplicationWorkState()).toEqual({
      ready: true,
      reasons: [],
    });
  });
});
