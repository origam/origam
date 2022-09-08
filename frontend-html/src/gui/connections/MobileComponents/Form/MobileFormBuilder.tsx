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
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getRowStateRowBgColor } from "model/selectors/RowState/getRowStateRowBgColor";
import { RadioButton } from "gui/Components/Form/RadioButton";
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { flow } from "mobx";
import { isReadOnly } from "model/selectors/RowState/isReadOnly";
import { DomEvent } from "leaflet";
import { getRowStateAllowRead } from "model/selectors/RowState/getRowStateAllowRead";
import { getRowStateMayCauseFlicker } from "model/selectors/RowState/getRowStateMayCauseFlicker";
import { CtxPanelVisibility } from "gui/contexts/GUIContexts";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { FieldDimensions } from "gui/Components/Form/FieldDimensions";
import { compareTabIndexOwners, ITabIndexOwner } from "model/entities/TabIndexOwner";
import { FormRoot } from "gui/Workbench/ScreenArea/FormView/FormRoot";
import "gui/connections/MobileComponents/Form/MobileForm.module.scss";
import { MobileFormField } from "gui/connections/MobileComponents/Form/MobileFormField";
import { MobileFormSection } from "gui/connections/MobileComponents/Form/MobileFormSection";
import { MobileCheckBox } from "gui/connections/MobileComponents/Form/CheckBox";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import { findStrings } from "xmlInterpreters/xmlUtils";
import { ExtraButtonsContext } from "gui/connections/MobileComponents/Navigation/DetailNavigator";
import { NavigationButton } from "gui/connections/MobileComponents/Navigation/NavigationButton";


@inject(({dataView}) => {
  return {dataView, xmlFormRootObject: dataView.formViewUI};
})
@observer
export class MobileFormBuilder extends React.Component<{
  xmlFormRootObject?: any;
  dataView?: IDataView;
}> {
  static contextType = CtxPanelVisibility

  componentDidMount() {
    document.addEventListener("click", event => this.notifyClick(event))
  }

  componentWillUnmount() {
    document.removeEventListener("click", event => this.notifyClick(event));
  }

  notifyClick(event: any) {
    this.props.dataView!.formFocusManager.setLastFocused(event.target);
  }

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

    function recursive(xfo: any, indexInParent: number): FormItem[] | undefined {
      if (xfo.name === "FormRoot") {
        return [new FormItem("-1",
          <FormRoot
            key={xfo.$iid}
            style={{backgroundColor}}
            className={"formRootMobile"}
          >
            {
              xfo.elements
                .flatMap((child: any, index: number) => recursive(child, index))
                .flat()
                .filter((item: any) => item)
                .sort(compareTabIndexOwners)
                .map((item: FormItem) => item.element)
            }
            <ExtraButtonsContext.Consumer>
              {
                extraButtons =>(
                  (extraButtons && (!extraButtons.node.dataView || extraButtons.node.dataView.isFormViewActive())) &&
                    extraButtons.node.children.map(node =>
                      <NavigationButton
                        key={node.name}
                        label={node.name}
                        onClick={() => extraButtons!.onNodeClick(node)}
                      />)
                )
              }
            </ExtraButtonsContext.Consumer>
          </FormRoot>
        )];
      } else if (xfo.name === "FormElement" && xfo.attributes.Type === "FormSection") {
        return [new FormItem((xfo.attributes.TabIndex)?.toString(),
          <MobileFormSection
            key={xfo.$iid}
            title={xfo.attributes.Title}
            backgroundColor={backgroundColor}
            foreGroundColor={foreGroundColor}
          >
            {
              xfo.elements
                .flatMap((child: any, index: number) => recursive(child, index))
                .flat()
                .filter((item: any) => item)
                .sort(compareTabIndexOwners)
                .map((item: FormItem) => item.element)
            }
          </MobileFormSection>
        )];
      } else if (xfo.name === "FormElement" && xfo.attributes.Type === "Label") {
        return undefined;
      } else if (xfo.name === "Control" && xfo.attributes.Column === "RadioButton") {
        const sourceField = getDataSourceFieldByName(self.props.dataView, xfo.attributes.Id);

        const checked = row
          ? String(dataTable.getCellValueByDataSourceField(row, sourceField!)) === xfo.attributes.Value
          : false;

        return [new FormItem(xfo.attributes.TabIndex,
          <RadioButton
            key={xfo.$iid}
            caption={xfo.attributes.Name}
            className={"formItem"}
            fieldDimensions={new FieldDimensions()}
            name={xfo.attributes.Id}
            value={xfo.attributes.Value}
            checked={checked}
            onKeyDown={(event) => self.onKeyDown(event)}
            subscribeToFocusManager={(radioInput) =>
              focusManager.subscribe(radioInput, xfo.attributes.Id, xfo.attributes.TabIndex)
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
        )];
      } else if (xfo.name === "PropertyNames") {
        return findPropertiesInPropertyNode(xfo, self.props.dataView)
          .map((property) => {
            return (new FormItem(property!.tabIndex,
              <Observer key={property!.id}>
                {() => {
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

                  const isHidden =
                    (!getRowStateAllowRead(property, rowId || "", property.id) ||
                      getRowStateMayCauseFlicker(property)) && !!row;

                  if (property.column === "CheckBox") {
                    return (
                      <Provider property={property}>
                        <MobileCheckBox
                          isHidden={isHidden}
                          checked={value}
                          readOnly={!row || isReadOnly(property, rowId)}
                          labelColor={foreGroundColor}
                        />
                      </Provider>
                    );
                  }

                  return (
                    <Provider property={property} key={property.id}>
                      <MobileFormField
                        isHidden={isHidden}
                        caption={property.name}
                        hideCaption={property.column === "Image"}
                        captionLength={property.captionLength}
                        captionColor={foreGroundColor}
                        dock={property.dock}
                        toolTip={property.toolTip}
                        value={value}
                        isRichText={property.isRichText}
                        textualValue={textualValue}
                        xmlNode={property.xmlNode}
                        backgroundColor={backgroundColor}
                        fieldDimensions={new FieldDimensions()}
                      />
                    </Provider>
                  );
                }}
              </Observer>
            ));
          });
      } else {
        return xfo.elements.map((child: any, index: number) => recursive(child, index));
      }
    }

    const form = recursive(this.props.xmlFormRootObject, 0)!
      .filter(item => item)
      .sort(compareTabIndexOwners)
      .map(item => item.element);

    if (this.props.dataView?.isFirst && this.context.isVisible) {
      focusManager.autoFocus();
    }
    return form;
  }

  render() {
    return this.buildForm();
  }
}

class FormItem implements ITabIndexOwner {
  constructor(
    public tabIndex: string | undefined,
    public element: JSX.Element) {
  }
}

function findPropertiesInPropertyNode(xfo: any, dataView: IDataView | undefined) {
  if (xfo.name !== "PropertyNames") {
    throw new Error("Nor a property node")
  }
  if (!dataView) {
    return [];
  }
  const row = getSelectedRow(dataView);
  const propertyNames = findStrings(xfo);
  return propertyNames
    .map(propertyId => {
      let property = getDataViewPropertyById(dataView, propertyId);
      if (row && property?.column === "Polymorph") {
        property = property.getPolymophicProperty(row);
      }
      return property;
    })
}