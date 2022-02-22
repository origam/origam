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
import { MobXProviderContext } from "mobx-react";
import { MobileState } from "model/entities/MobileState/MobileState";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { reaction } from "mobx";
import styleObject from "gui/connections/MobileComponents/mobile.module.scss"

export class MobileStyleSheetLoader extends React.Component<{}> {

  static contextType = MobXProviderContext;

  get application(): MobileState {
    return this.context.application;
  }

  disposer: (()=>void) | undefined;

  componentDidMount() {

    this.disposer = reaction(
      ()=> isMobileLayoutActive(this.application),
      (mobileLayoutActive)=> {
        const identifierClass = "mobileStyleSheetIdentifier";
        const sheet = Array.from(document.styleSheets)
          .find((sheet: any) => sheet.rules[0] && sheet.rules[0].selectorText === "."+styleObject[identifierClass] )
        sheet!.disabled = !mobileLayoutActive;
      },
      {fireImmediately: true}
    );
  }

  componentWillUnmount() {
    this.disposer?.();
  }

  render() {
    return (
        <></>
    );
  }
}



