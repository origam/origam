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
import "gui/connections/MobileComponents/Form/MobileForm.module.scss";
import { FormField, ICaptionPosition } from "gui/Components/Form/FormField";
import { IDockType } from "model/entities/types/IProperty";
import { FieldDimensions } from "gui/Components/Form/FieldDimensions";

export class MobileFormField extends React.Component<{
  isHidden?: boolean;
  caption: React.ReactNode;
  hideCaption?: boolean;
  captionLength: number;
  captionPosition?: ICaptionPosition;
  captionColor?: string;
  dock?: IDockType;
  toolTip?: string;
  value?: any;
  isRichText: boolean;
  textualValue?: any;
  xmlNode?: any;
  backgroundColor?: string;
}> {
  render() {
    return (
      <div className={"formItem"}>
        <FormField
          isHidden={this.props.isHidden}
          caption={this.props.caption}
          hideCaption={this.props.hideCaption}
          captionLength={this.props.captionLength}
          captionPosition={this.props.captionPosition}
          captionColor={this.props.captionColor}
          dock={this.props.dock}
          toolTip={this.props.toolTip}
          value={this.props.value}
          isRichText={this.props.isRichText}
          textualValue={this.props.textualValue}
          xmlNode={this.props.xmlNode}
          backgroundColor={this.props.backgroundColor}
          fieldDimensions={new FieldDimensions()}
          linkInForm={true}
        />
      </div>
    );
  }
}



