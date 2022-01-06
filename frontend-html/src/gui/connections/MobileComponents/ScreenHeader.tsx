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

import React, { useContext } from "react";
import S from "./ScreenHeader.module.scss";
import { MobXProviderContext, observer } from "mobx-react";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { getIsTopmostNonDialogScreen } from "model/selectors/getIsTopmostNonDialogScreen";

export const ScreenHeader: React.FC<{}> = observer((props) => {

  const application = useContext(MobXProviderContext).application;
  const openedScreenItems = getOpenedNonDialogScreenItems(application);
  const activeScreen =  openedScreenItems.find((item) => getIsTopmostNonDialogScreen(item));

  if(!activeScreen || !activeScreen.content.formScreen?.workflowTaskId){
    return null;
  }

  return (
    <div className={S.root}>
      {activeScreen.formTitle}
    </div>
  );
});
