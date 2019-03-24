import {
  findAllStopping,
  findAll,
  findFirstBFS,
  findFirstDFS
} from "src/util/xmlObj";

export function getNearestReprs(node: any, reprs: Map<any, any>) {
  return findAllStopping(node, (cn: any) => cn !== node && reprs.has(cn)).map(
    (cn: any) => reprs.get(cn)
  );
}

export const findFormRoots = (startNode: any) =>
  findAll(startNode, (ch: any) => ch.name === "FormRoot");

export const findFormRootsStopping = (startNode: any) =>
  findAllStopping(startNode, (ch: any) => ch.name === "FormRoot");

export const findFormSections = (startNode: any) =>
  findAll(
    startNode,
    (ch: any) =>
      ch.name === "FormElement" && ch.attributes.Type === "FormSection"
  );

export const findGridProps = (startNode: any) =>
  findAll(
    startNode,
    (ch: any) => ch.name === "Property" && ch.parent.name === "Properties"
  );

export const findGridPropsStopping = (startNode: any) =>
  findAllStopping(
    startNode,
    (ch: any) => ch.name === "Property" && ch.parent.name === "Properties"
  );

export const findDropDownProps = (startNode: any) =>
  findAll(
    startNode,
    (ch: any) => ch.name === "Property" && ch.parent.name === "DropDownColumns"
  );

export const findFormFields = (startNode: any) =>
  findAll(
    startNode,
    (ch: any) => ch.name === "string" && ch.parent.name === "PropertyNames"
  );

export const findWindows = (startNode: any) =>
  findAll(startNode, (cn: any) => cn.name === "Window");

export const findUIRoots = (startNode: any) =>
  findAll(startNode, (cn: any) => cn.name === "UIRoot");

export const findUIChildren = (startNode: any) =>
  findFirstBFS(startNode, (cn: any) => cn.name === "UIChildren");

export const findChildBoxes = (startNode: any) => {
  const children = findUIChildren(startNode);
  return children.elements.filter(
    (ch: any) => ch.name === "UIElement" && ch.attributes.Type === "Box"
  );
};

export const findBoxes = (startNode: any) =>
  findAll(
    startNode,
    (ch: any) =>
      (ch.name === "UIElement" || ch.name === "UIRoot") &&
      ch.attributes.Type === "Box"
  );

export const findVBoxes = (startNode: any) =>
  findAll(
    startNode,
    (ch: any) =>
      (ch.name === "UIElement" || ch.name === "UIRoot") &&
      ch.attributes.Type === "VBox"
  );

export const findTabbedPanels = (startNode: any) =>
  findAll(
    startNode,
    (ch: any) =>
      (ch.name === "UIElement" || ch.name === "UIRoot") &&
      ch.attributes.Type === "Tab"
  );

export const findLabels = (startNode: any) =>
  findAll(
    startNode,
    (ch: any) =>
      (ch.name === "UIElement" || ch.name === "UIRoot") &&
      ch.attributes.Type === "Label"
  );

export const findDataViews = (startNode: any) =>
  findAll(
    startNode,
    (ch: any) =>
      (ch.name === "UIElement" || ch.name === "UIRoot") &&
      ch.attributes.Type === "Grid"
  );

export const findDataViewsStopping = (startNode: any) =>
  findAllStopping(
    startNode,
    (ch: any) =>
      (ch.name === "UIElement" || ch.name === "UIRoot") &&
      ch.attributes.Type === "Grid"
  );

export const findHSplits = (startNode: any) =>
  findAll(
    startNode,
    (ch: any) =>
      (ch.name === "UIElement" || ch.name === "UIRoot") &&
      ch.attributes.Type === "HSplit"
  );

export const findVSplits = (startNode: any) =>
  findAll(
    startNode,
    (ch: any) =>
      (ch.name === "UIElement" || ch.name === "UIRoot") &&
      ch.attributes.Type === "VSplit"
  );
