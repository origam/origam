/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

import { askYesNoQuestion } from "gui/Components/Dialog/DialogUtils";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";
import { T } from "utils/translation";

export function questionCancelWorkflow(ctx: any) {
  return askYesNoQuestion(
    ctx,
    getOpenedScreen(ctx).tabTitle,
    T("Are you sure you want to cancel the workflow?", "cancel_workflow_confirmation")
  );
}