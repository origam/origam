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
import S from "gui/connections/MobileComponents/MenuButton.module.scss";
import { Icon } from "gui/Components/Icon/Icon";
import { SidebarAlertCounter } from "gui/Components/Sidebar/AlertCounter";
import { getWorkQueuesTotalItemsCount } from "model/selectors/WorkQueues/getWorkQueuesTotalItemCount";
import { MobXProviderContext, observer } from "mobx-react";

export const MenuButton: React.FC<{}> = observer((props) => {

  const context = useContext(MobXProviderContext)

  const workQueuesItemsCount = getWorkQueuesTotalItemsCount(context.application);
  return (
    <div
      className={S.root}
      onClick={() => context.application.mobileState.hamburgerClick()}
    >
      <Icon
        src={"./icons/noun-hamburger-4238597.svg"}
        className={S.menuIcon}
      />
      {workQueuesItemsCount > 0 && (
        <SidebarAlertCounter>{workQueuesItemsCount}</SidebarAlertCounter>
      )}
    </div>
  );
});
