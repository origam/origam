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

import { Observer } from "mobx-react";
import React, { useEffect, useMemo } from "react";
import cx from 'classnames';
import S from "gui/Components/Dropdown/Dropdown.module.scss";
import { MobileDropdownBehavior } from "gui/connections/MobileComponents/Form/ComboBox/MobileDropdownBehavior";
import { IDropdownEditorData } from "modules/Editors/DropdownEditor/DropdownEditorData";

export function MobileDropdownEditorInput(props: {
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
  behavior: MobileDropdownBehavior,
  isLink?: boolean,
  editorData: IDropdownEditorData
}) {
  const refInput = useMemo(() => {
    return (elm: any) => {
      props.behavior.refInputElement(elm);
    };
  }, [props.behavior]);

  useEffect(() => {
    props.behavior.updateTextOverflowState();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
  function getStyle() {
    if (props.customStyle) {
      return props.customStyle;
    } else {
      return {
        color: props.foregroundColor,
        backgroundColor: props.backgroundColor,
      };
    }
  }

  return (
    <Observer>
      {() => (
        <input
          className={cx("input", S.input, {isLink: props.isLink})}
          ref={refInput}
          placeholder={props.editorData.isResolving ? "Loading..." : ""}
          onChange={props.behavior.handleInputChange}
          value={props.behavior.inputValue || ""}
          style={getStyle()}
          autoComplete={"new-password"}
          autoCorrect={"off"}
          autoCapitalize={"off"}
          spellCheck={"false"}
        />
      )}
    </Observer>
  );
}
