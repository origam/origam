/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

import { observable, runInAction } from "mobx";
import { FormScreenLifecycle02 } from "model/entities/FormScreenLifecycle/FormScreenLifecycle";

jest.mock("uuid", () => ({ v4: () => "00000000-0000-0000-0000-000000000000" }));
jest.mock("xmlInterpreters/screenXml", () => ({}));

const autoRefreshIntervalSeconds = 3;
const createdLifecycles: FormScreenLifecycle02[] = [];

function createAutoRefreshingScreen() {
  const openedScreen = observable.object(
    {
      $type_IOpenedScreen: 1,
      isActive: true,
      isClosed: false,
      isSleeping: false,
      isSleepingDirty: false,
      content: { isLoading: false, formScreen: undefined as any },
    },
    {},
    { deep: false }
  );
  const formScreen = observable.object(
    {
      $type_IFormScreen: 1,
      autoRefreshInterval: autoRefreshIntervalSeconds,
      suppressSave: false,
      isDirty: false,
      dataViews: [],
      parent: openedScreen,
    },
    {},
    { deep: false }
  );
  openedScreen.content.formScreen = formScreen;
  const lifecycle = new FormScreenLifecycle02();
  (lifecycle as any).parent = formScreen;
  createdLifecycles.push(lifecycle);
  return { lifecycle, openedScreen };
}

function runGenerator(generator: Generator) {
  let step = generator.next();
  while (!step.done) {
    step = generator.next();
  }
}

describe("FormScreenLifecycle02 auto refresh", () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    createdLifecycles.splice(0).forEach(lifecycle => lifecycle.disposers.forEach(disposer => disposer()));
    jest.clearAllTimers();
    jest.useRealTimers();
  });

  test("keeps a single interval when the auto refresh reaction runs again", () => {
    const { lifecycle, openedScreen } = createAutoRefreshingScreen();
    const performAutoReload = jest.spyOn(lifecycle, "performAutoReload").mockImplementation(() => {});
    runGenerator(lifecycle.startAutoRefreshIfNeeded());

    runInAction(() => {
      openedScreen.isActive = false;
      openedScreen.isActive = true;
    });
    jest.advanceTimersByTime(autoRefreshIntervalSeconds * 1000);
    expect(performAutoReload).toHaveBeenCalledTimes(1);

    lifecycle.clearAutoRefreshInterval();
    jest.advanceTimersByTime(3 * autoRefreshIntervalSeconds * 1000);
    expect(performAutoReload).toHaveBeenCalledTimes(1);
    expect(jest.getTimerCount()).toBe(0);
  });

  test("stops the interval of a closed screen without refreshing its session", () => {
    const { lifecycle, openedScreen } = createAutoRefreshingScreen();
    const refreshSession = jest.spyOn(lifecycle, "refreshSession").mockImplementation(function* () {});
    runGenerator(lifecycle.startAutoRefreshIfNeeded());

    openedScreen.isClosed = true;
    jest.advanceTimersByTime(3 * autoRefreshIntervalSeconds * 1000);

    expect(refreshSession).not.toHaveBeenCalled();
    expect(jest.getTimerCount()).toBe(0);
  });

  test("keeps refreshing an open screen", () => {
    const { lifecycle } = createAutoRefreshingScreen();
    const refreshSession = jest.spyOn(lifecycle, "refreshSession").mockImplementation(function* () {});
    runGenerator(lifecycle.startAutoRefreshIfNeeded());

    jest.advanceTimersByTime(2 * autoRefreshIntervalSeconds * 1000);

    expect(refreshSession).toHaveBeenCalledTimes(2);
    expect(jest.getTimerCount()).toBe(1);
  });
});
