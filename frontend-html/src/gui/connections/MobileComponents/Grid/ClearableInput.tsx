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
import S from "gui/connections/MobileComponents/Grid/ClearableInput.module.scss";
import { MobXProviderContext } from "mobx-react";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { InputClearButton } from "gui/connections/MobileComponents/Grid/InputClearButton";

interface IClearableInputData{
  id?: string;
  className?: string;
  value?: string;
  onChange?: (event: any) => void;
  onBlur?: (event: any) => void;
}

export const ClearableInput = React.forwardRef<HTMLInputElement, IClearableInputData>((props, ref) => {

  const application = useContext(MobXProviderContext).application;

  if (!isMobileLayoutActive(application)) {
    return <input
      id={props.id}
      className={props.className}
      value={props.value}
      autoComplete={"off"}
      onChange={props.onChange}
      onBlur={props.onBlur}
      ref={ref}
    />
  }

  function onClearClick() {
    const event = {target: {value: ""}};
    props.onChange?.(event);
  }

  return (
    <div className={S.root}>
      <input
        id={props.id}
        className={props.className}
        value={props.value}
        autoComplete={"off"}
        onChange={props.onChange}
        onBlur={props.onBlur}
        ref={ref}
      />
      <InputClearButton
        visible={props.value !== ""}
        onClick={onClearClick}
      />
    </div>
  );
});
