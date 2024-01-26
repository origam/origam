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

import { getActiveScreen } from "model/selectors/getActiveScreen";
import { IFormScreen } from "model/entities/types/IFormScreen";
import { getIsSuppressSave } from "model/selectors/FormScreen/getIsSuppressSave";
import { getIsSuppressRefresh } from "model/selectors/FormScreen/getIsSuppressRefresh";

export function getScreenActionButtonsState(ctx: any): IScreenActionButtonsState | undefined{
  const activeScreen = getActiveScreen(ctx);
  if (activeScreen && !activeScreen.content) return undefined;
  const formScreen =
    activeScreen && !activeScreen.content.isLoading ? activeScreen.content.formScreen : undefined;
  const isDirty = formScreen && formScreen.isDirty;
  let actionButtonsVisible = !!formScreen;

  return {
    actionButtonsVisible: actionButtonsVisible,
    formScreen: formScreen,
    isSaveButtonVisible: actionButtonsVisible && !getIsSuppressSave(formScreen),
    isRefreshButtonVisible: actionButtonsVisible && !getIsSuppressRefresh(formScreen),
    isDirty: !!isDirty,
  };
}

export interface IScreenActionButtonsState {
  actionButtonsVisible: boolean;
  formScreen: IFormScreen | undefined;
  isSaveButtonVisible: boolean;
  isRefreshButtonVisible: boolean;
  isDirty: boolean;
}