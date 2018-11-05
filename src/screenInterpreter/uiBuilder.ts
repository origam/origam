import * as React from "react";

import { Box } from "../uiComponents/skeleton/Box";
import { FormField } from "../uiComponents/skeleton/FormField";
import { FormSection } from "../uiComponents/skeleton/FormSection";
import { Grid } from "../uiComponents/skeleton/Grid";
import { HSplit } from "../uiComponents/skeleton/HSplit";
import { Tab } from "../uiComponents/skeleton/Tab";
import { TabHandle } from "../uiComponents/skeleton/TabHandle";
import { VBox } from "../uiComponents/skeleton/VBox";
import { VSplit } from "../uiComponents/skeleton/VSplit";
import { Window } from "../uiComponents/skeleton/Window";
import { GridTable } from "src/uiComponents/controls/GridTable";
import { GridForm } from "src/uiComponents/controls/GridForm";

const elementMap = {
  Box,
  FormField,
  FormSection,
  Grid,
  HSplit,
  Tab,
  TabHandle,
  VBox,
  VSplit,
  Window,
  Table: GridTable,
  Form: GridForm
};

export function buildReactTree(treeStruct: any) {
  let props = treeStruct.props;
  if (treeStruct.type === "Tab") {
    props = {
      ...props,
      handles: props.handles.map((handle: any) => buildReactTree(handle))
    };
  } else if (treeStruct.type === "Grid") {
    props = {
      ...props,
      form: props.form.map((child: any) => buildReactTree(child)),
      table: props.table.map((child: any) => buildReactTree(child))
    };
  } else if (treeStruct.type === "TabHandle") {
    props = {
      ...props,
      key: props.id
    }
  }
  return React.createElement(
    elementMap[treeStruct.type],
    props,
    ...treeStruct.children.map((child: any) => buildReactTree(child))
  );
}
