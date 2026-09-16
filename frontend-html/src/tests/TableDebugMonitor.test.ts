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

import {
  clearTableDebugValues,
  setTableDebugRendered,
} from "gui/Components/ScreenElements/Table/TableRendering/DebugTableMonitor";

function dataViewContext(modelInstanceId: string) {
  return { $type_IDataView: 1, modelInstanceId };
}

describe("table debug monitor", () => {
  beforeEach(() => {
    jest.useFakeTimers();
    delete (window as any).tableDebugMonitor;
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  test("reports each table render independently and invalidates stale renders", () => {
    const firstTable = dataViewContext("first");
    const secondTable = dataViewContext("second");

    clearTableDebugValues(firstTable);
    setTableDebugRendered(firstTable);
    clearTableDebugValues(secondTable);
    setTableDebugRendered(secondTable);

    jest.advanceTimersByTime(200);

    expect((window as any).tableDebugMonitor).toMatchObject({
      first: { rendered: true, renderRevision: 1, renderedRevision: 1 },
      second: { rendered: true, renderRevision: 1, renderedRevision: 1 },
    });

    clearTableDebugValues(firstTable);

    expect((window as any).tableDebugMonitor.first).toMatchObject({
      data: [],
      rendered: false,
      renderRevision: 2,
      renderedRevision: 1,
    });
  });
});
