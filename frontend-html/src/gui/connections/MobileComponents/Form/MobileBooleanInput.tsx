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

import S from "gui/connections/MobileComponents/Form/MobileBooleanInput.module.scss";

export const MobileBooleanInput: React.FC<{
  checked: boolean | null | undefined;
  onChange: (event: IBooleanEvent)=> void;
  disabled?: boolean;
}> = (props) => {

  function onClick(){
    if(!props.disabled){
      props.onChange({
        target: {checked: !props.checked},
        type: "click"
      })
    }
  }

  function getStateClass(){
    if(props.checked === null || props.checked === undefined){
      return S.undefined;
    }
    if(props.checked){
      return S.on;
    }
    return S.off;
  }

  return (
    <div
      className={S.root}
      onClick={onClick}
    >
      <div className={S.slot + " " + getStateClass() + " " + (props.disabled ? S.readOnly : "")}>
        <div className={S.nob}/>
      </div>
    </div>
  );
}

export interface IBooleanEvent {
  target: IBooleanTarget;
  type: string
}

export interface IBooleanTarget {
  checked: boolean
}
