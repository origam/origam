import { inject, observer, Observer, Provider } from "mobx-react";
import React from "react";
import { IDataView } from "../../../../model/entities/types/IDataView";
import { getDataTable } from "../../../../model/selectors/DataView/getDataTable";
import { getDataViewPropertyById } from "../../../../model/selectors/DataView/getDataViewPropertyById";
import { getSelectedRow } from "../../../../model/selectors/DataView/getSelectedRow";
import { findStrings } from "../../../../xmlInterpreters/screenXml";

import { FormRoot } from "./FormRoot";
import { FormViewEditor } from "./FormViewEditor";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getRowStateRowBgColor } from "model/selectors/RowState/getRowStateRowBgColor";
import { FormField } from "gui02/components/Form/FormField";
import { FormSection } from "gui02/components/Form/FormSection";
import { FormLabel } from "gui02/components/Form/FormLabel";
import { RadioButton } from "gui02/components/Form/RadioButton";
import { getDataSourceFieldByName } from "../../../../model/selectors/DataSources/getDataSourceFieldByName";
import { getFormScreenLifecycle } from "../../../../model/selectors/FormScreen/getFormScreenLifecycle";
import { flow } from "mobx";
import { getRowStateAllowUpdate } from "../../../../model/selectors/RowState/getRowStateAllowUpdate";
import { CheckBox } from "../../../../gui02/components/Form/CheckBox";
import {isReadOnly} from "../../../../model/selectors/RowState/isReadOnly";
import {DomEvent} from "leaflet";

@inject(({ dataView }) => {
  return { dataView, xmlFormRootObject: dataView.formViewUI };
})
@observer
export class FormBuilder extends React.Component<{
  xmlFormRootObject?: any;
  dataView?: IDataView;
}> {
  onKeyDown(event: any){
    if (event.key === "Tab") {
      DomEvent.preventDefault(event);
      if(event.shiftKey){
        this.props.dataView!.focusManager.focusPrevious(document.activeElement);
      }
      else{
        this.props.dataView!.focusManager.focusNext(document.activeElement);
      }
      return;
    }
  }

  buildForm() {
    const self = this;
    const row = getSelectedRow(this.props.dataView);
    const rowId = getSelectedRowId(this.props.dataView);
    const dataTable = getDataTable(this.props.dataView);
    let backgroundColor: string | undefined;
    if (row && rowId) {
      backgroundColor = getRowStateRowBgColor(self.props.dataView, rowId);
    }
    const focusManager = self.props.dataView!.focusManager;

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
      } else if (xfo.name === "Control" && xfo.attributes.Column === "RadioButton") {
        const sourceField = getDataSourceFieldByName(self.props.dataView, xfo.attributes.Id);

        const checked = row
          ? dataTable.getCellValueByDataSourceField(row, sourceField!) === xfo.attributes.Value
          : false;

        return (
          <RadioButton
            key={xfo.$iid}
            caption={xfo.attributes.Name}
            left={+xfo.attributes.X}
            top={+xfo.attributes.Y}
            width={+xfo.attributes.Width}
            height={+xfo.attributes.Height}
            name={xfo.attributes.Id}
            value={xfo.attributes.Value}
            checked={checked}
            onKeyDown={(event) => self.onKeyDown(event)}
            subscribeToFocusManager={(radioInput) =>
              focusManager.subscribe(radioInput, xfo.attributes.Id, xfo.attributes.TabIndex)
            }
            onSelected={(value) => {
              const formScreenLifecycle = getFormScreenLifecycle(self.props.dataView);
              flow(function* () {
                yield* formScreenLifecycle.updateRadioButtonValue(
                  self.props.dataView!,
                  row,
                  xfo.attributes.Id,
                  value
                );
              })();
            }}
          />
        );
      } else if (xfo.name === "PropertyNames") {
        const propertyNames = findStrings(xfo);
        return propertyNames.map((propertyId) => {
          return (
            <Observer key={propertyId}>
              {() => {
                let property = getDataViewPropertyById(self.props.dataView, propertyId);
                if(row && property?.column === "Polymorph"){
                  property = property.getPolymophicProperty(row);
                }
                let value;
                let textualValue = value;
                if (row && property) {
                  value = dataTable.getCellValue(row, property);
                  if (property.isLookup) {
                    textualValue = dataTable.getCellText(row, property);
                  }
                }
                if (!property) {
                  return <></>;
                }

                if (property.column === "CheckBox") {
                  return (
                    <Provider property={property}>
                      <CheckBox
                        checked={value}
                        readOnly={!row || isReadOnly(property, rowId)}
                        onKeyDown={(event) => self.onKeyDown(event)}
                        subscribeToFocusManager={(radioInput) =>
                          focusManager.subscribe(radioInput, property!.id, property!.tabIndex)
                        }
                      />
                    </Provider>
                  );
                }

                return (
                  <Provider property={property} key={property.id}>
                    <FormField
                      caption={property.name}
                      captionLength={property.captionLength}
                      captionPosition={property.captionPosition}
                      dock={property.dock}
                      height={property.height}
                      width={property.width}
                      left={property.x}
                      top={property.y}
                      editor={
                        <FormViewEditor
                          value={value}
                          isRichText={property.isRichText}
                          textualValue={textualValue}
                          xmlNode={property.xmlNode}
                        />
                      }
                    />
                  </Provider>
                );
              }}
            </Observer>
          );
        });
      } else {
        return xfo.elements.map((child: any) => recursive(child));
      }
    }

    const form = recursive(this.props.xmlFormRootObject);
    if(this.props.dataView?.isFirst){
      focusManager.autoFocus();
    }
    return form;
  }

  render() {
    return this.buildForm();
  }
}
