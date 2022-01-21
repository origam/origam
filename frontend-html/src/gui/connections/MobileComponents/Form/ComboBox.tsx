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
import S from "./ComboBox.module.scss";
import cx from "classnames";
import CS from "@origam/components/src/components/Dropdown/Dropdown.module.scss";
import { MobXProviderContext, observer } from "mobx-react";
import { MobileState } from "model/entities/MobileState/MobileState";
import { ComboEditLayoutState } from "model/entities/MobileState/MobileLayoutState";
import { IFocusable } from "model/entities/FormFocusManager";
import { XmlBuildDropdownEditorInternal } from "modules/Editors/DropdownEditor/DropdownEditor";
import { ComboFullScreenEditor } from "gui/connections/MobileComponents/Form/ComboFullScreenEditor";
import { IDataView } from "model/entities/types/IDataView";
import { IProperty } from "model/entities/types/IProperty";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { MobileDropdownBehavior } from "gui/connections/MobileComponents/Form/MobileDropdownBehavior";


export interface IComboBoxProps {
  xmlNode: any;
  isReadOnly: boolean;
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
  tagEditor?: JSX.Element;
  isLink?: boolean;
  autoSort?: boolean;
  onTextOverflowChanged?: (toolTip: string | null | undefined) => void;
  onDoubleClick?: (event: any) => void;
  onClick?: (event: any) => void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  dataView: IDataView;
  property: IProperty;
  onKeyDown?(event: any): void;
}

export const ComboBox: React.FC<IComboBoxProps> = observer((props) => {

  const mobileState = useContext(MobXProviderContext).application.mobileState as MobileState;
  const row = getSelectedRow(props.property);
  const currentValue = row && props.dataView.dataTable.getCellText(row, props.property);

  return (
    <div
      className={cx(CS.control, S.mobileInput)}
      onClick={() => {
        mobileState.layoutState = new ComboEditLayoutState(
          <XmlBuildDropdownEditorInternal
            {...props}
            control={
              <ComboFullScreenEditor {...props}/>
            }
            mobileBehavior={true}
            makeBehavior={data => new MobileDropdownBehavior(data)}
          />)
      }}
    >
      <div className={"input " + S.input}>{currentValue}</div>
      <div className={cx("inputBtn", "lastOne")}>
        <i className="fas fa-caret-down"/>
      </div>
    </div>
  );
});




