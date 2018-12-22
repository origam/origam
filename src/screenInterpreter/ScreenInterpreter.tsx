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
import { Label } from "src/uiComponents/skeleton/Label";
import { TreePanel } from "src/uiComponents/controls/TreePanel";

export function reactProcessNode(node: any, path: any[]) {
  const nextNode = () =>
    node.elements.map((element: any) =>
      reactProcessNode(element, [...path, element])
    );

  const attr = node.attributes;

  if (path.length === 0) {
    // Root node
    return <>{nextNode()}</>;
  }
  switch (node.name) {
    case "FormRoot":
      break;
    case "Window":
      return (
        <Window id={attr.Id} name={attr.Title}>
          {nextNode()}
        </Window>
      );
    case "Property":
      break;
    case "Properties":
      break;
    case "FormField":
      break;
    case "PropertyNames":
      break;
    case "DataSources":
      break;
    case "ComponentBindings":
      break;
    case "UIRoot":
      break;
    default:
      console.log(node);
      throw new Error("Unknown screen node.");
  }
  return null;
}
