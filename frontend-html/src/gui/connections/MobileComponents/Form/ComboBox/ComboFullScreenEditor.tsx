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

import React, { useContext, useEffect } from "react";
import S from "gui/connections/MobileComponents/Form/ComboBox/ComboFullScreenEditor.module.scss";
import { IComboBoxProps } from "gui/connections/MobileComponents/Form/ComboBox/ComboBox";
import cx from "classnames";
import CS from "@origam/components/src/components/Dropdown/Dropdown.module.scss";
import { CtxDropdownRefCtrl } from "@origam/components";
import { DropdownEditorTable } from "modules/Editors/DropdownEditor/DropdownEditorBody";
import { MobileDropdownBehavior } from "gui/connections/MobileComponents/Form/ComboBox/MobileDropdownBehavior";
import { DropdownColumnDrivers, DropdownDataTable } from "modules/Editors/DropdownEditor/DropdownTableModel";
import { MobileDropdownEditorInput } from "gui/connections/MobileComponents/Form/ComboBox/MobileDropdownEditorInput";
import { IDropdownEditorData } from "modules/Editors/DropdownEditor/DropdownEditorData";


interface IComboFullScreenEditorProps extends IComboBoxProps{
  behavior: MobileDropdownBehavior;
  dataTable: DropdownDataTable;
  columnDrivers: DropdownColumnDrivers
  editorData: IDropdownEditorData
}

export const ComboFullScreenEditor: React.FC<IComboFullScreenEditorProps> = (props) => {

  const ref = useContext(CtxDropdownRefCtrl);

  useEffect(()=>{
    props.behavior.handleInputChange({target:{value:""}});
    props.behavior.makeFocused();
  });

  return (
    <div className={cx(CS.control, S.root)} ref={ref}>
      <MobileDropdownEditorInput
        backgroundColor={props.backgroundColor}
        foregroundColor={props.foregroundColor}
        customStyle={props.customStyle}
        isLink={props.isLink}
        behavior={props.behavior}
        editorData={props.editorData}
      />
      <DropdownEditorTable
        drivers={props.columnDrivers}
        dataTable={props.dataTable}
        rectCtrl={{
          top: 0,
          left: 0,
          width: window.screen.width,
          height: 0,
          bottom: 0,
          right: 0,
        }}
        beh={props.behavior}
        rowHeight={30}
      />
    </div>
  );
};




