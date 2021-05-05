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

import {IDialogInfo} from "../entities/types/IOpenedScreen";
import {OpenedScreen} from "../entities/OpenedScreen";
import {IFormScreenEnvelope} from "../entities/types/IFormScreen";
import {IMainMenuItemType} from "../entities/types/IMainMenu";

export function *createOpenedScreen(
  ctx: any,
  menuItemId: string,
  menuItemType: IMainMenuItemType,
  order: number,
  title: string,
  content: IFormScreenEnvelope,
  lazyLoading: boolean,
  dialogInfo: IDialogInfo | undefined,
  parameters: { [key: string]: any },
  isSleeping?: boolean,
  isSleepingDirty?: boolean
): Generator {
  return new OpenedScreen({
    menuItemId,
    menuItemType,
    order,
    title,
    content,
    dialogInfo,
    lazyLoading,
    parameters,
    isSleeping,
    isSleepingDirty
  });
}
