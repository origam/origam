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

import { MobXProviderContext, observer, Provider } from "mobx-react";
import { IApplication } from "model/entities/types/IApplication";
import React from "react";
import { CWorkbenchPage } from "gui/connections/pages/CWorkbenchPage";
import { ApplicationDialogStack } from "gui/Components/Dialog/DialogStack";
import { IWorkbench } from "model/entities/types/IWorkbench";

@observer
export class CMain extends React.Component {
  static contextType = MobXProviderContext;

  get application(): IApplication {
    return this.context.application;
  }

  get workbench(): IWorkbench {
    return this.application.workbench!;
  }

  render() {
    return (
      <div className={"toplevelContainer"}>
        <Provider workbench={this.workbench}>
          <ApplicationDialogStack/>
          <CWorkbenchPage/>
        </Provider>
      </div>
    );
  }
}
