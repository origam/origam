import { inject, observer, Observer, Provider } from "mobx-react";
import React from "react";
import { IDataView } from "../../../../model/entities/types/IDataView";
import { getDataTable } from "../../../../model/selectors/DataView/getDataTable";
import { getDataViewPropertyById } from "../../../../model/selectors/DataView/getDataViewPropertyById";
import { getSelectedRow } from "../../../../model/selectors/DataView/getSelectedRow";
import { findStrings } from "../../../../xmlInterpreters/screenXml";

import { FormRoot } from "./FormRoot";
import { FormViewEditor } from "./FormViewEditor";
import { getRowStates } from "model/selectors/RowState/getRowStates";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getRowStateRowBgColor } from "model/selectors/RowState/getRowStateRowBgColor";
import { FormField } from "gui02/components/Form/FormField";
import { FormSection } from "gui02/components/Form/FormSection";
import { FormLabel } from "gui02/components/Form/FormLabel";

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
    const row = getSelectedRow(this.props.dataView);
    const rowId = getSelectedRowId(this.props.dataView);
    const dataTable = getDataTable(this.props.dataView);
    let backgroundColor: string | undefined;
    if (row && rowId) {
      backgroundColor = getRowStateRowBgColor(self.props.dataView, rowId);
    }

    function recursive(xfo: any) {
      if (xfo.name === "FormRoot") {
        return (
          <FormRoot key={xfo.$iid} style={{ backgroundColor }}>
            {xfo.elements.map((child: any) => recursive(child))}
          </FormRoot>
        );
      } else if (xfo.name === "FormElement" && xfo.attributes.Type === "FormSection") {
        return (
          <FormSection
            key={xfo.$iid}
            width={parseInt(xfo.attributes.Width, 10)}
            height={parseInt(xfo.attributes.Height, 10)}
            left={parseInt(xfo.attributes.X, 10)}
            top={parseInt(xfo.attributes.Y, 10)}
            title={xfo.attributes.Title}
          >
            {xfo.elements.map((child: any) => recursive(child))}
          </FormSection>
        );
      } else if (xfo.name === "FormElement" && xfo.attributes.Type === "Label") {
        return (
          <FormLabel
            key={xfo.$iid}
            title={xfo.attributes.Title}
            left={+xfo.attributes.X}
            top={+xfo.attributes.Y}
            width={+xfo.attributes.Width}
            height={+xfo.attributes.Height}
          />
        );
      } else if (xfo.name === "PropertyNames") {
        const propertyNames = findStrings(xfo);
        return propertyNames.map((propertyId) => {
          return (
            <Observer key={propertyId}>
              {() => {
                const property = getDataViewPropertyById(self.props.dataView, propertyId);
                let value;
                let textualValue = value;
                if (row && property) {
                  value = dataTable.getCellValue(row, property);
                  if (property.isLookup) {
                    textualValue = dataTable.getCellText(row, property);
                  }
                }

                return property ? (
                  <Provider property={property}>
                    <FormField
                      // Id={property.id}
                      caption={property.name}
                      captionLength={property.captionLength}
                      captionPosition={property.captionPosition}
                      dock={property.dock}
                      // Column={property.column}
                      // Entity={property.entity}
                      height={property.height}
                      width={property.width}
                      left={property.x}
                      top={property.y}
                      isCheckbox={property.column === "CheckBox"}
                      editor={<FormViewEditor value={value} textualValue={textualValue} />}
                    />
                  </Provider>
                ) : (
                  <></>
                );
              }}
            </Observer>
          );
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
