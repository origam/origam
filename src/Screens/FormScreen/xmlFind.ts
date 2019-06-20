import { findStopping } from "../../utils/xml";

export const findGrids = (node: any) =>
  findStopping(
    node,
    n =>
      (n.name === "UIElement" || n.name === "UIRoot") &&
      n.attributes.Type === "Grid"
  );

export const findProps = (node: any) =>
  findStopping(
    node,
    n => n.name === "Property" && n.parent.name === "Properties"
  );

export const findDropDownProps = (node: any) =>
  findStopping(
    node,
    n => n.name === "Property" && n.parent.name === "DropDownColumns"
  );

export const findFormRoot = (node: any) =>
  findStopping(
    node,
    n => n.name === "FormRoot" && n.attributes.Type === "Canvas"
  )[0];

export const findBindings = (node: any) =>
  findStopping(
    node,
    n => n.name === "Binding" && n.parent.name === "ComponentBindings"
  );

export const findWindow = (node: any) =>
  findStopping(node, n => n.name === "Window")[0];

export const findUIRoot = (node: any) =>
  findStopping(node, n => n.name === "UIRoot")[0];

export const findScreenElements = (node: any) =>
  findStopping(node, n => n.name === "UIRoot" || n.name === "UIElement");

export const findFormElements = (node: any) =>
  findStopping(
    node,
    (n: any) =>
      n.name === "FormRoot" ||
      n.name === "FormElement" ||
      (n.type === "text" && n.parent.name === "string")
  );

export const findPropertyIds = (node: any) =>
  findStopping(
    node,
    n =>
      n !== node &&
      n.type === "text" &&
      n.parent.name === "string" &&
      n.parent.parent.name === "PropertyNames"
  ).map(e => e.text);

export const findDataSources = (node: any) =>
  findStopping(
    node,
    n => n.parent && n.parent.name === "DataSources" && n.name === "DataSource"
  );

export const findDataSourceFields = (node: any) =>
  findStopping(node, n => n.name === "Field");

// TODO: IsRootGrid / IsRootEntity?
export const isSessionedScreen = (win: any) => {
  return win.attributes.UseSession === "true"
}

