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

import { flow } from "mobx";
import { handleError } from "model/actions/handleError";

export function wrapInFlowWithHandler(args: { ctx: any; action: (() => Promise<any>) | (() => void) }) {
  return flow(function*runWithHandler() {
    try {
      yield args.action();
    } catch (e) {
      yield*handleError(args.ctx)(e);
      throw e;
    }
  });
}

export function runInFlowWithHandler(args: { ctx: any, action: (() => Promise<any>) | (() => void) }) {
  return wrapInFlowWithHandler(args)();
}

export function runGeneratorInFlowWithHandler(args: { ctx: any, generator: Generator }) {
  return flow(function*runWithHandler() {
    try {
      yield*args.generator;
    } catch (e) {
      yield*handleError(args.ctx)(e);
      throw e;
    }
  })();
}