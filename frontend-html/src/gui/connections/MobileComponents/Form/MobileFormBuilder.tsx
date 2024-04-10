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
import { compareTabIndexOwners, ITabIndexOwner, TabIndex } from "model/entities/TabIndexOwner";
import { FormRoot } from "gui/Workbench/ScreenArea/FormView/FormRoot";
import "gui/connections/MobileComponents/Form/MobileForm.scss";
import { MobileFormField } from "gui/connections/MobileComponents/Form/MobileFormField";
import { MobileFormSection } from "gui/connections/MobileComponents/Form/MobileFormSection";
import { MobileCheckBox } from "gui/connections/MobileComponents/Form/CheckBox";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import { findStrings } from "xmlInterpreters/xmlUtils";
import { ExtraButtonsContext } from "gui/connections/MobileComponents/Navigation/DetailNavigator";
import { NavigationButton } from "gui/connections/MobileComponents/Navigation/NavigationButton";
import { IProperty } from "model/entities/types/IProperty";
import { MobileState } from "model/entities/MobileState/MobileState";
import { getAllActions } from "model/selectors/DataView/getMobileActions";
import { IActionType } from "model/entities/types/IAction";
import { MobileAction, MobileActionLink } from "gui/connections/MobileComponents/Form/MobileAction";


@inject(({dataView}) => {
  return {dataView, xmlFormRootObject: dataView.formViewUI};
})
@observer
export class MobileFormBuilder extends React.Component<{
  mobileState: MobileState
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

    function recursiveParse(xfo: any, parent: FormItem | null): FormItem[] | undefined {
      if (xfo.name === "FormRoot") {
        let formItem = new FormItem(TabIndex.Min,
          parent,
          xfo,
          []
        );
        formItem.children = xfo.elements
          .flatMap((child: any, index: number) => recursiveParse(child, formItem))
          .flat()
          .filter((item: any) => item)
          .sort(compareByIsFormSectionThenByTabIndex);
        return [formItem];
      } else if (xfo.name === "FormElement" && xfo.attributes.Type === "FormSection") {
        const formItem = new FormItem((xfo.attributes.TabIndex)?.toString(),
          parent,
          xfo,
          [],
          undefined,
          true);
        formItem.children = xfo.elements
          .flatMap((child: any, index: number) => recursiveParse(child, formItem))
          .flat()
          .filter((item: any) => item)
          .sort(compareByIsFormSectionThenByTabIndex);
        return [formItem];
      } else if (xfo.name === "FormElement" && xfo.attributes.Type === "Label") {
        return undefined;
      } else if (xfo.name === "Control" && xfo.attributes.Column === "RadioButton") {
        return [new FormItem(xfo.attributes.TabIndex, parent, xfo, [])];
      } else if (xfo.name === "PropertyNames") {
        return findPropertiesInPropertyNode(xfo, self.props.dataView)
          .map((property) => new FormItem(property!.tabIndex, parent, xfo, [], property));
      } else {
        return xfo.elements.map((child: any) => recursiveParse(child, parent));
      }
    }

    function recursiveBuild(formItem: FormItem): JSX.Element | undefined {
      if (formItem.xfo.name === "FormRoot") {
        const actions = getAllActions(self.props.dataView, self.props.mobileState);
        const noGroupActions = actions.filter(action => !action.groupId)

        return (
          <FormRoot
            key={formItem.xfo.$iid}
            dataView={self.props.dataView!}
            style={{backgroundColor}}
            className={"formRootMobile"}
          >
            {
              formItem.children
                .flatMap((child: any) => recursiveBuild(child))
                .flat()
                .filter((item: any) => item)
            }
            <div key={"divider1"} style={{minHeight: "20px", maxHeight: "20px"}}/>
            <ExtraButtonsContext.Consumer>
              {
                extraButtons => (
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
            {noGroupActions.length > 0 && <div key={"divider2"} style={{minHeight: "20px", maxHeight: "20px"}}/>}
            {noGroupActions.map(action =>
              action.type === IActionType.Dropdown
              ? <MobileActionLink
                  key={action.id}
                  linkAction={action}
                  actions={actions.filter(subAction => subAction.groupId === action.id)}
                  mobileState={self.props.mobileState}/>
              : <MobileAction
                  key={action.id}
                  action={action}
                  mobileState={self.props.mobileState}/>
            )}
          </FormRoot>
        );
      } else if (formItem.xfo.name === "FormElement" && formItem.xfo.attributes.Type === "FormSection") {
        return (
          <MobileFormSection
            key={formItem.xfo.$iid}
            title={formItem.xfo.attributes.Title}
            startOpen={isFirstFormSection(formItem)}
            backgroundColor={backgroundColor}
            foreGroundColor={foreGroundColor}
          >
            {
              formItem.children
                .flatMap((child: any, index: number) => recursiveBuild(child))
                .flat()
                .filter((item: any) => item)
            }
          </MobileFormSection>
        );
      } else if (formItem.xfo.name === "FormElement" && formItem.xfo.attributes.Type === "Label") {
        return undefined;
      } else if (formItem.xfo.name === "Control" && formItem.xfo.attributes.Column === "RadioButton") {
        const sourceField = getDataSourceFieldByName(self.props.dataView, formItem.xfo.attributes.Id);
        const checked = row
          ? String(dataTable.getCellValueByDataSourceField(row, sourceField!)) === formItem.xfo.attributes.Value
          : false;

        return (
          <RadioButton
            key={formItem.xfo.$iid}
            caption={formItem.xfo.attributes.Name}
            className={"formItem"}
            fieldDimensions={new FieldDimensions()}
            name={formItem.xfo.attributes.Id}
            value={formItem.xfo.attributes.Value}
            checked={checked}
            onKeyDown={(event) => self.onKeyDown(event)}
            subscribeToFocusManager={(radioInput) =>
              focusManager.subscribe(radioInput, formItem.xfo.attributes.Id, TabIndex.create(formItem.xfo.attributes.TabIndex))
            }
            labelColor={foreGroundColor}
            onClick={() => self?.props?.dataView?.formFocusManager.stopAutoFocus()}
            onSelected={(value) => {
              const formScreenLifecycle = getFormScreenLifecycle(self.props.dataView);
              flow(function*() {
                yield*formScreenLifecycle.updateRadioButtonValue(
                  self.props.dataView!,
                  row,
                  formItem.xfo.attributes.Id,
                  value
                );
              })();
            }}
          />);
      } else if (formItem.xfo.name === "PropertyNames") {
        const property = formItem.property;
        return (<Observer key={property!.id}>
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
                  tooltip={property.tooltip}
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
        </Observer>);
      } else {
        return formItem.xfo.elements.map((child: any) => recursiveBuild(child));
      }
    }

    const topItems = recursiveParse(this.props.xmlFormRootObject, null)!
      .filter(item => item)
      .sort(compareByIsFormSectionThenByTabIndex);
    if (topItems.length !== 1) {
      return null;
    }
    const topItem = topItems[0];
    const form = recursiveBuild(topItem);

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
    public tabIndex: TabIndex,
    public parent: FormItem | null,
    public xfo: any,
    public children: FormItem[],
    public property: IProperty | undefined = undefined,
    public isFormSection: boolean = false) {
  }
}

function isFirstFormSection(formItem: FormItem) {
  let parent = formItem.parent;
  while (parent) {
    if (parent.children.indexOf(formItem) !== 0) {
      return false;
    }
    parent = parent.parent;
  }
  return true;
}

function compareByIsFormSectionThenByTabIndex(x: FormItem, y: FormItem) {
  if (x.isFormSection && !y.isFormSection) {
    return 1;
  }
  if (!x.isFormSection && y.isFormSection) {
    return -1;
  }
  return compareTabIndexOwners(x, y);
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