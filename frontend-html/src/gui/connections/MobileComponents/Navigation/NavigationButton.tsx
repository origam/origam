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
import S from "gui/connections/MobileComponents/Navigation/NavigationButton.module.scss";
import { Icon } from "gui/Components/Icon/Icon";
import SN from "gui/connections/MobileComponents/Navigation/NavigationButton.module.scss";

export const NavigationButton: React.FC<{
  label: string;
  onClick: () => void;
  isOpen?: boolean;
}> = (props) => {

  return (
    <div className={props.isOpen ? S.openRoot : ""}>
      <div
        className={S.navigationButton}
        onClick={props.onClick}
      >
        <div className={S.label}>
          {props.label}
        </div>
        <Icon
          src={props.isOpen ? "./icons/noun-chevron-933246.svg" : "./icons/noun-chevron-933251.svg"}
          className={SN.navigationIcon}
        />
      </div>
      {props.children}
    </div>
  );
};