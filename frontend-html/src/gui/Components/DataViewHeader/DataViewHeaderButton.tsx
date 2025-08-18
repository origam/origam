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
import S from "gui/Components/DataViewHeader/DataViewHeaderButton.module.scss";
import cx from "classnames";

export const DataViewHeaderButton: React.FC<{
  domRef?: any;
  isHidden?: boolean;
  disabled?: boolean;
  title?: string;
  id?: string;
  onClick?(event: any): void;
}> = (props) => (
  <button
    id={props.id}
    title={props.title}
    ref={props.domRef}
    className={cx(props.disabled ? S.disabled : S.enabled, S.root, {hidden: props.isHidden})}
    onClick={props.onClick}
    disabled={props.disabled}
  >
    {props.children}
  </button>
);
