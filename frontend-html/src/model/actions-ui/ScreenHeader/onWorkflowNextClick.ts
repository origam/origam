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
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";

export function onWorkflowNextClick(ctx: any) {
  return flow(function*onWorkflowNextClick(event: any) {
    try {
      const lifecycle = getFormScreenLifecycle(ctx);
      if(lifecycle.workflowNextActive > 0) return;
      yield*lifecycle.onFlushData();
      yield*lifecycle.onWorkflowNextClick(event);
    } catch (e) {
      yield*handleError(ctx)(e);
      throw e;
    }
  });
}
