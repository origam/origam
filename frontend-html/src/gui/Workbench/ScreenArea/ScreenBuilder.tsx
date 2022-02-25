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

import { observer, Provider } from "mobx-react";
import React from "react";
import { IOpenedScreen } from "../../../model/entities/types/IOpenedScreen";
import { FormScreenBuilder } from "gui/Workbench/ScreenArea/FormScreenBuilder/FormScreenBuilder";


@observer
export class DialogScreenBuilder extends React.Component<{
  openedScreen: IOpenedScreen;
}> {
  render() {
    const {openedScreen} = this.props;
    const {content} = openedScreen;
    if (!content.isLoading) {
      return (
        <Provider formScreen={content}>
          {!content.isLoading && (
            <>
              <FormScreenBuilder
                title={content.formScreen!.title}
                xmlWindowObject={content.formScreen!.screenUI}
              />
            </>
          )}
        </Provider>
      );
    } else {
      return null;
    }
  }
}
