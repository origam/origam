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

import { inject, observer, Observer, Provider } from "mobx-react";
import React from "react";
import { IDataView } from "model/entities/types/IDataView";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { FormRoot } from "./FormRoot";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getRowStateRowBgColor } from "model/selectors/RowState/getRowStateRowBgColor";
import { FormField, getTooltip } from "gui/Components/Form/FormField";
import { FormSection } from "gui/Components/Form/FormSection";
import { FormLabel } from "gui/Components/Form/FormLabel";
import { RadioButton } from "gui/Components/Form/RadioButton";
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { flow } from "mobx";
import { CheckBox } from "gui/Components/Form/CheckBox";
import { isReadOnly } from "model/selectors/RowState/isReadOnly";
import { DomEvent } from "leaflet";
import { getRowStateAllowRead } from "model/selectors/RowState/getRowStateAllowRead";
import { getRowStateMayCauseFlicker } from "model/selectors/RowState/getRowStateMayCauseFlicker";
import { CtxPanelVisibility } from "gui/contexts/GUIContexts";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { dimensionsFromProperty, dimensionsFromXmlNode } from "gui/Components/Form/FieldDimensions";
import { findStrings } from "xmlInterpreters/xmlUtils";
import { TabIndex } from "model/entities/TabIndexOwner";
import { BackupFocusPlaceHolder } from "gui/Workbench/ScreenArea/FormView/BackupFocusPlaceHolder";


@inject(({dataView}) => {
  return {dataView, xmlFormRootObject: dataView.formViewUI};
})
@observer
export class FormBuilder extends React.Component<{
  xmlFormRootObject?: any;
  dataView?: IDataView;
}> {
  static contextType = CtxPanelVisibility

  onKeyDown(event: any) {
    if (event.key === "Tab") {
      DomEvent.preventDefault(event);
      if (event.shiftKey) {
        this.props.dataView!.formFocusManager.focusPrevious(document.activeElement);
      } else {
        this.props.dataView!.formFocusManager.focusNext(document.activeElement);
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
    let foreGroundColor: string | undefined;
    if (row && rowId) {
      backgroundColor = getRowStateRowBgColor(self.props.dataView, rowId);
      foreGroundColor = getRowStateForegroundColor(
        self.props.dataView,
        rowId || ""
      );
    }
    const focusManager = self.props.dataView!.formFocusManager;

    function recursive(xfo: any) {
      if (xfo.name === "FormRoot") {
        return (
          <FormRoot
            key={xfo.$iid}
            dataView={self.props.dataView!}
            style={{backgroundColor}}
          >
            {xfo.elements.map((child: any) => recursive(child))}
            <BackupFocusPlaceHolder ctx={self.props.dataView}/>
          </FormRoot>
        );
      } else if (xfo.name === "FormElement" && xfo.attributes.Type === "FormSection") {
        return (
          <FormSection
            key={xfo.$iid}
            dimensions={dimensionsFromXmlNode(xfo)}
            title={xfo.attributes.Title}
            backgroundColor={backgroundColor}
            foreGroundColor={foreGroundColor}
          >
            {xfo.elements.map((child: any) => recursive(child))}
          </FormSection>
        );
      } else if (xfo.name === "FormElement" && xfo.attributes.Type === "Label") {
        return (
          <FormLabel
            key={xfo.$iid}
            title={xfo.attributes.Title}
            fieldDimensions={dimensionsFromXmlNode(xfo)}
            foregroundColor={foreGroundColor}
          />
        );
      } else if (xfo.name === "Control" && xfo.attributes.Column === "RadioButton") {
        const sourceField = getDataSourceFieldByName(self.props.dataView, xfo.attributes.Id);

        const checked = row
          ? String(dataTable.getCellValueByDataSourceField(row, sourceField!)) === xfo.attributes.Value
          : false;

        return (
          <RadioButton
            key={xfo.$iid}
            caption={xfo.attributes.Name}
            fieldDimensions={dimensionsFromXmlNode(xfo)}
            name={xfo.attributes.Id}
            value={xfo.attributes.Value}
            checked={checked}
            onKeyDown={(event) => self.onKeyDown(event)}
            subscribeToFocusManager={(radioInput) =>
              focusManager.subscribe(radioInput, xfo.attributes.Id, TabIndex.create(xfo.attributes.TabIndex))
            }
            labelColor={foreGroundColor}
            onClick={() => self?.props?.dataView?.formFocusManager.stopAutoFocus()}
            onSelected={(value) => {
              const formScreenLifecycle = getFormScreenLifecycle(self.props.dataView);
              flow(function*() {
                yield*formScreenLifecycle.updateRadioButtonValue(
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
                if (row && property?.column === "Polymorph") {
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

                let mayCauseFlicker = getRowStateMayCauseFlicker(property);
                let rowStateAllowRead = getRowStateAllowRead(property, rowId || "", property.id);
                const isHidden = (!rowStateAllowRead || mayCauseFlicker) && !!row;

                  if (property.column === "CheckBox") {
                    return (
                      <Provider property={property}>
                        <div title={getTooltip(property.tooltip)}>
                           <CheckBox
                            fieldDimensions={dimensionsFromProperty(property)}
                            isHidden={isHidden}
                            checked={value}
                            readOnly={!row || isReadOnly(property, rowId)}
                            onKeyDown={(event) => self.onKeyDown(event)}
                            subscribeToFocusManager={(radioInput) =>
                              focusManager.subscribe(radioInput, property!.id, property!.tabIndex)
                            }
                            onClick={() => self?.props?.dataView?.formFocusManager.stopAutoFocus()}
                            labelColor={foreGroundColor}
                            />
                        </div>
                      </Provider>
                    );
                  }

                  return (
                    <Provider property={property} key={property.id}>
                      <FormField
                        isHidden={isHidden}
                        caption={property.name}
                        hideCaption={property.column === "Image"}
                        captionLength={property.captionLength}
                        captionPosition={property.captionPosition}
                        captionColor={foreGroundColor}
                        dock={property.dock}
                        fieldDimensions={dimensionsFromProperty(property)}
                        tooltip={property.tooltip}
                        value={value}
                        isRichText={property.isRichText}
                        textualValue={textualValue}
                        xmlNode={property.xmlNode}
                        backgroundColor={backgroundColor}
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
    if (this.props.dataView?.isFirst && this.context.isVisible) {
      focusManager.autoFocus();
    }
    return form;
  }

  render() {
    return this.buildForm();
  }
}
