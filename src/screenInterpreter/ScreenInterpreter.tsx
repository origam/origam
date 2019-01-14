import * as React from "react";

import { Box } from "../uiComponents/skeleton/Box";
import { FormField } from "../uiComponents/skeleton/FormField";
import { FormSection } from "../uiComponents/skeleton/FormSection";
import { DataView } from "../uiComponents/skeleton/DataView";
import { HSplit } from "../uiComponents/skeleton/HSplit";
import { Tab } from "../uiComponents/skeleton/Tab";
import { TabHandle } from "../uiComponents/skeleton/TabHandle";
import { VBox } from "../uiComponents/skeleton/VBox";
import { VSplit } from "../uiComponents/skeleton/VSplit";
import { Window } from "../uiComponents/skeleton/Window";

import { GridForm } from "src/uiComponents/controls/GridForm";
import { Label } from "src/uiComponents/skeleton/Label";
import { TreePanel } from "src/uiComponents/controls/TreePanel";
import { IDropDownColumn, IProperty } from "./types";
import {
  IXmlNode,
  ICollectPropertiesContext,
  ICollectDropDownColumnsContext
} from "./types";
import GridTable from "src/uiComponents/controls/GridTable2/Table";
import { IGridDimensions } from "src/uiComponents/controls/GridTable2/types";
import bind from "bind-decorator";
import { CPR } from "src/utils/canvas";
import { computed } from "mobx";
import { IRenderHeader } from "../uiComponents/controls/GridTable2/types";
import TableCnd from "src/uiComponents/controls/GridTable2/TableCnd";


function parseAttrRect(attr: {
  [key: string]: string;
}): { w: number; h: number; x: number; y: number } {
  return {
    x: parseInt(attr.X, 10),
    y: parseInt(attr.Y, 10),
    w: parseInt(attr.Width, 10),
    h: parseInt(attr.Height, 10)
  };
}

export function collectProperties(
  node: IXmlNode,
  path: IXmlNode[],
  originalContext: ICollectPropertiesContext
): any {
  const nextNode = () =>
    (node.elements || []).map((element: any) =>
      collectProperties(element, [...path, element], context)
    );

  let context = originalContext;
  const attr = node.attributes;

  switch (node.name) {
    case "Property": {
      if (path.slice(-2)[0].name === "Properties") {
        const dropDownColumns: IDropDownColumn[] = [];
        collectDropDownColumns(node, [...path, node], { dropDownColumns });
        context.properties.push({
          id: attr.Id,
          modelInstanceId: attr.ModelInstanceId,
          name: attr.Name,
          readOnly: { true: true, false: false }[attr.ReadOnly],
          entity: attr.Entity,
          column: attr.Column,
          ...parseAttrRect(attr),
          captionLength: parseInt(attr.CaptionLength, 10),
          captionPosition: attr.CaptionPosition,
          dropDownColumns
        });
      }
      return nextNode();
    }
    default:
      return nextNode();
  }
}

export function collectDropDownColumns(
  node: IXmlNode,
  path: IXmlNode[],
  originalContext: ICollectDropDownColumnsContext
): any {
  const nextNode = () =>
    (node.elements || []).map((element: any) =>
      collectDropDownColumns(element, [...path, element], context)
    );

  let context = originalContext;
  const attr = node.attributes;

  switch (node.name) {
    case "Property": {
      if (path.slice(-2)[0].name === "DropDownColumns") {
        context.dropDownColumns.push({
          id: attr.Id,
          name: attr.Name,
          entity: attr.Entity,
          column: attr.Column,
          index: parseInt(attr.Index, 10)
        });
      }
      return nextNode();
    }
    default:
      return nextNode();
  }
}

function buildForm(
  node: IXmlNode,
  path: IXmlNode[],
  context: { properties: IProperty[] }
): React.ReactNode {
  const nextNode = () =>
    (node.elements || []).map((element: any) =>
      buildForm(element, [...path, element], context)
    );

  const attr = node.attributes;

  switch (node.name) {
    case "FormElement":
      switch (attr.Type) {
        case "FormSection":
          return (
            <FormSection {...parseAttrRect(attr)} title={attr.Title}>
              {nextNode()}
            </FormSection>
          );
        default:
          return nextNode();
      }
    case "string":
      if (path.slice(-2)[0].name === "PropertyNames") {
        const property = context.properties.find(
          p => p.id === node.elements[0].text
        );
        if (property) {
          switch (property.column) {
            case "Text":
              return <FormField property={property} />;
            case "Currency":
              return <FormField property={property} />;
            case "Date":
              return <FormField property={property} />;
            case "CheckBox":
              return <FormField property={property} />;
            case "ComboBox":
              return <FormField property={property} />;
            default:
              return null;
          }
        }
      } else {
        return nextNode();
      }
    default:
      return nextNode();
  }
}





export function buildUI(node: IXmlNode, path: IXmlNode[]): React.ReactNode {
  const nextNode = () =>
    (node.elements || []).map((element: any) =>
      buildUI(element, [...path, element])
    );

  const attr = node.attributes;

  if (path.length === 0) {
    // Root node
    return <>{nextNode()}</>;
  }
  switch (node.name) {
    case "Window":
      return (
        <Window id={attr.Id} name={attr.Title}>
          {nextNode()}
        </Window>
      );
    case "UIElement":
    case "UIRoot":
      switch (attr.Type) {
        case "Grid":
          console.log(node);
          const properties: IProperty[] = [];
          collectProperties(node, [...path, node], { properties });
          console.log(properties);
          return (
            <DataView properties={properties}>
              {/*<GridForm>
                {buildForm(node, [...path, node], { properties })}
              </GridForm>*/}
              <TableCnd />
            </DataView>
          );
        case "VSplit":
          return (
            <VSplit id={attr.Id} modelInstanceId={attr.ModelInstanceId}>
              {nextNode()}
            </VSplit>
          );
        case "HSplit":
          return (
            <HSplit id={attr.Id} modelInstanceId={attr.ModelInstanceId}>
              {nextNode()}
            </HSplit>
          );
        case "VBox":
          return (
            <VBox
              id={attr.Id}
              modelInstanceId={attr.ModelInstanceId}
              {...parseAttrRect(attr)}
            >
              {nextNode()}
            </VBox>
          );
        default:
          return nextNode();
      }
    default:
      return nextNode();
  }
  return null;
}
