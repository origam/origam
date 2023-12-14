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

import React, { useEffect, useState } from "react";
import S from "gui/connections/MobileComponents/Form/ComboBox/ComboFullScreenEditor.module.scss";
import cx from "classnames";
import CS from "gui/Components/Dropdown/Dropdown.module.scss";
import { DropdownEditorTable } from "modules/Editors/DropdownEditor/DropdownEditorBody";
import { MobileDropdownBehavior } from "gui/connections/MobileComponents/Form/ComboBox/MobileDropdownBehavior";
import { DropdownColumnDrivers, DropdownDataTable } from "modules/Editors/DropdownEditor/DropdownTableModel";
import { MobileDropdownEditorInput } from "gui/connections/MobileComponents/Form/ComboBox/MobileDropdownEditorInput";
import { IDropdownEditorData } from "modules/Editors/DropdownEditor/DropdownEditorData";
import Measure from "react-measure";


interface IComboFullScreenEditorProps {
  behavior: MobileDropdownBehavior;
  dataTable: DropdownDataTable;
  columnDrivers: DropdownColumnDrivers;
  editorData: IDropdownEditorData;
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
  isLink?: boolean,
}

export const ComboFullScreenEditor: React.FC<IComboFullScreenEditorProps> = (props) => {

  useEffect(()=>{
    props.behavior.handleInputChange({target:{value:""}});
    props.behavior.makeFocused();
  });

  const [height, setHeight] = useState<number>(150);

  return (
    <Measure
      bounds
      onResize={contentRect => {
        setHeight(contentRect.bounds?.height ?? 150);
      }}
    >
      {({ measureRef }) => (
        <div className={cx(CS.control, S.root)} ref={measureRef}>
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
            height={height - 25} // 25 is height of MobileDropdownEditorInput
          />
        </div>
      )}
    </Measure>
  );
};




