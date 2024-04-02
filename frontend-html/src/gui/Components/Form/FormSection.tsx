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
import "gui/Components/Form/FormSection.scss";
import cx from "classnames";
import { FormSectionHeader } from "gui/Components/Form/FormSectionHeader";
import { FieldDimensions } from "gui/Components/Form/FieldDimensions";

export const FormSection: React.FC<{
  title?: string;
  backgroundColor: string | undefined;
  foreGroundColor: string | undefined;
  dimensions: FieldDimensions;
}> = (props) => {
  const hasTitle = !!props.title;
  return (
    <div
      className={cx("formSection", {hasTitle})}
      style={getStyle(props.dimensions, props.backgroundColor)}
    >
      {hasTitle && (
        <FormSectionHeader foreGroundColor={props.foreGroundColor} tooltip={props.title}>
          {props.title}
        </FormSectionHeader>
      )}
      {props.children}
    </div>
  );
};

export function getStyle(dimensions: FieldDimensions, backgroundColor: string | undefined) {
  const style = dimensions.asStyle();
  style["backgroundColor"] = backgroundColor;
  return style;
}