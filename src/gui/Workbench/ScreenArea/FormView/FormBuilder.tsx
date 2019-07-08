import React from "react";

import { FormSection } from "../../../Components/ScreenElements/FormSection";
import { observer, inject } from "mobx-react";
import { findStrings } from "../../../../xmlInterpreters/screenXml";
import { getDataViewPropertyById } from "../../../../model/selectors/DataView/getDataViewPropertyById";
import { IDataView } from "../../../../model/types/IDataView";
import { FormField } from "./FormField";
import { FormRoot } from "../../../Components/ScreenElements/FormRoot";

@inject(({ dataView }) => {
  return { dataView, xmlFormRootObject: dataView.formViewUI };
})
@observer
export class FormBuilder extends React.Component<{
  xmlFormRootObject?: any;
  dataView?: IDataView;
}> {
  buildForm() {
    const self = this;
    function recursive(xfo: any) {
      if (xfo.name === "FormRoot") {
        return (
          <FormRoot>
            {xfo.elements.map((child: any) => recursive(child))}
          </FormRoot>
        );
      } else if (
        xfo.name === "FormElement" &&
        xfo.attributes.Type === "FormSection"
      ) {
        return (
          <FormSection
            width={parseInt(xfo.attributes.Width, 10)}
            height={parseInt(xfo.attributes.Height, 10)}
            x={parseInt(xfo.attributes.X, 10)}
            y={parseInt(xfo.attributes.Y, 10)}
            title={xfo.attributes.Title}
          >
            {xfo.elements.map((child: any) => recursive(child))}
          </FormSection>
        );
      } else if (xfo.name === "PropertyNames") {
        const propertyNames = findStrings(xfo);
        return propertyNames.map(propertyId => {
          const property = getDataViewPropertyById(
            self.props.dataView,
            propertyId
          );
          return property ? (
            <FormField
              Id={property.id}
              Name={property.name}
              CaptionLength={property.captionLength}
              CaptionPosition={property.captionPosition}
              Column={property.column}
              Entity={property.entity}
              Height={property.height}
              Width={property.width}
              X={property.x}
              Y={property.y}
            />
            
          ) : null;
        });
      } else {
        return xfo.elements.map((child: any) => recursive(child));
      }
    }
    return recursive(this.props.xmlFormRootObject);
  }

  render() {
    // debugger
    return this.buildForm();
  }
}
