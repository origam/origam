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

import { MobXProviderContext, observer } from "mobx-react";
import { IFormScreen } from "model/entities/types/IFormScreen";
import React from "react";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { desktopRecursiveBuilder } from "gui/Workbench/ScreenArea/FormScreenBuilder/desktopRecursiveBuilder";
import { mobileRecursiveBuilder } from "gui/Workbench/ScreenArea/FormScreenBuilder/mobileRecursiveBuilder";
import { ComponentFactory } from "gui/Workbench/ScreenArea/FormScreenBuilder/ComponentFactory";
import { findUIRoot } from "xmlInterpreters/xmlUtils";

@observer
export class FormScreenBuilder extends React.Component<{
  title: string;
  xmlWindowObject: any;
}> {
  static contextType = MobXProviderContext;

  get formScreen(): IFormScreen {
    return this.context.formScreen.formScreen;
  }

  render() {
    const uiRoot = findUIRoot(this.props.xmlWindowObject);

    if(isMobileLayoutActive(this.formScreen)){
      return mobileRecursiveBuilder({
        uiRoot: uiRoot,
        formScreen: this.formScreen,
        title: this.props.title,
        desktopRecursiveBuilder: desktopRecursiveBuilder,
        componentFactory: new ComponentFactory(),
      });
    }
    else
    {
      return desktopRecursiveBuilder(this.formScreen, uiRoot);
    }
  }
}


