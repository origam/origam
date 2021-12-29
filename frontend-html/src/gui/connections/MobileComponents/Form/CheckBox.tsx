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
import { IFocusable } from "model/entities/FormFocusManager";
import { FieldDimensions } from "gui/Components/Form/FieldDimensions";
import { CheckBox } from "gui/Components/Form/CheckBox";
import cx from "classnames";
import S from "gui/connections/MobileComponents/Form/CheckBox.module.scss"

export const MobileCheckBox: React.FC<{
  isHidden?: boolean;
  checked: boolean;
  readOnly: boolean;
  onKeyDown: (event: any) => void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  onClick: () => void;
  labelColor?: string;
}> = (props) => {
  return (
    <div className={cx(S.root, "formItem")}>
      <CheckBox
        isHidden={props.isHidden}
        checked={props.checked}
        readOnly={props.readOnly}
        onKeyDown={props.onKeyDown}
        subscribeToFocusManager={props.subscribeToFocusManager}
        onClick={props.onClick}
        labelColor={props.labelColor}
        fieldDimensions={new FieldDimensions()}
      />
    </div>
  );
};
