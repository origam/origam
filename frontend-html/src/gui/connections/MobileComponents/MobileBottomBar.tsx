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

import React from "react";
import S from "./MobileBottomBar.module.scss";
import { BottomIcon } from "gui/connections/MobileComponents/BottomIcon";
import { MobileState } from "model/entities/MobileState";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { DataViewHeaderAction } from "gui/Components/DataViewHeader/DataViewHeaderAction";
import { Dropdown } from "gui/Components/Dropdown/Dropdown";
import { DropdownItem } from "gui/Components/Dropdown/DropdownItem";
import { onScreenTabHandleClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabHandleClick";
import { Icon } from "@origam/components";

export class MobileBottomBar extends React.Component<{
  mobileState: MobileState
}> {
  render() {
    return (
      <div className={S.root}>
       <BottomIcon
         iconPath={"./icons/noun-close-25798.svg"}
         onClick={()=> {this.props.mobileState.close()}}
       />
        {/*<Dropdowner*/}
        {/*  trigger={({refTrigger, setDropped}) => (*/}
        {/*    <DataViewHeaderAction*/}
        {/*      refDom={refTrigger}*/}
        {/*      onMouseDown={() => setDropped(true)}*/}
        {/*      isActive={false}*/}
        {/*    >*/}
        {/*     <Icon src={"./icons/noun-right-1784045.svg"} />*/}
        {/*    </DataViewHeaderAction>*/}
        {/*  )}*/}
        {/*  content={({setDropped}) => (*/}
        {/*    <Dropdown>*/}
        {/*      {openedScreens.map(openScreen =>*/}
        {/*        <DropdownItem*/}
        {/*          key={openScreen.tabTitle}*/}
        {/*          onClick={(event: any) => {*/}
        {/*            setDropped(false);*/}
        {/*            onScreenTabHandleClick(openScreen)(event)*/}
        {/*          }}*/}
        {/*        >*/}
        {/*          {openScreen.tabTitle}*/}
        {/*        </DropdownItem>*/}
        {/*      )}*/}
        {/*    </Dropdown>*/}
        {/*  )}*/}
        {/*/>*/}
        <BottomIcon
         iconPath={"./icons/noun-right-1784045.svg"}
         onClick={()=> {}}
       />
        <BottomIcon
         iconPath={"./icons/noun-loading-1780489.svg"}
         onClick={()=> {}}
       />
        <BottomIcon
         iconPath={"./icons/noun-save-1014816.svg"}
         onClick={()=> {}}
       />
      </div>
    );
  }
}



