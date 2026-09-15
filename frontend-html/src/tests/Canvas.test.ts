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

import React from "react";
import { render } from "@testing-library/react";
import { Canvas } from "gui/Components/ScreenElements/Table/Canvas";

jest.mock("utils/canvas", () => ({
  CPR: () => 1,
}));

afterEach(() => {
  jest.restoreAllMocks();
});

test("requests a redraw after committing new canvas dimensions", () => {
  const context = {} as CanvasRenderingContext2D;
  jest.spyOn(HTMLCanvasElement.prototype, "getContext").mockReturnValue(context);
  const committedSizes: [number, number][] = [];
  const onSizeCommitted = jest.fn(() => {
    const canvas = document.querySelector("canvas")!;
    committedSizes.push([canvas.width, canvas.height]);
  });
  const refCanvasElement = jest.fn();

  const { rerender } = render(React.createElement(Canvas, {
    width: 100,
    height: 50,
    refCanvasElement,
    onSizeCommitted,
  }));
  onSizeCommitted.mockClear();

  rerender(React.createElement(Canvas, {
    width: 200,
    height: 75,
    refCanvasElement,
    onSizeCommitted,
  }));

  expect(onSizeCommitted).toHaveBeenCalledTimes(1);
  expect(onSizeCommitted).toHaveBeenCalledWith(context);
  expect(committedSizes).toEqual([[200, 75]]);
  expect(document.querySelector("canvas")).toMatchObject({
    width: 200,
    height: 75,
  });
});
