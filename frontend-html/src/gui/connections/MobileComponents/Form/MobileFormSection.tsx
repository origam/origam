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

import React, { useState } from "react";
import "gui/connections/MobileComponents/Form/MobileFormSection.module.scss";
import { FieldDimensions } from "gui/Components/Form/FieldDimensions";
import { getStyle } from "gui/Components/Form/FormSection";
import cx from "classnames";
import { Icon } from "gui/Components/Icon/Icon";
import S from "gui/connections/MobileComponents/Form/MobileFormSection.module.scss";

const emptyDimensions = new FieldDimensions();

export const MobileFormSection: React.FC<{
  title?: string;
  startOpen: boolean;
  backgroundColor: string | undefined;
  foreGroundColor: string | undefined;
}> = (props) => {

  const [isOpen, setOpen] = useState(props.startOpen);
  const hasTitle = !!props.title;

  return (
    <div>
      <div
        className={cx(S.formSection, {hasTitle})}
        style={getStyle(emptyDimensions, props.backgroundColor)}
      >
        <div
          className={S.header}
          onClick={() => setOpen(!isOpen)}
        >
          {hasTitle && (
            <h1 className={S.title} style={{color: props.foreGroundColor}} title={props.title}>
              {props.title}
            </h1>
          )}
          <Icon
            src={isOpen ? "./icons/noun-chevron-933254.svg" : "./icons/noun-chevron-933246.svg"}
            className={S.navigationIcon}
          />
        </div>
        {isOpen && props.children}
      </div>
    </div>
  );
};
