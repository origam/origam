import * as React from "react";
import * as xmlJs from "xml-js";
import {
  Menu,
  Submenu,
  MenuItemWorkflow,
  MenuItemForm
} from "./MainMenuComponent";

function reactProcessNode(node: any) {
  switch (node.name) {
    case undefined:
    const rn = node.elements.map((element: any) => reactProcessNode(element));
    console.log(rn)
      return (
        <>{node.elements.map((element: any) => reactProcessNode(element))}</>
      );
    case "Menu":
      return (
        <Menu label={node.attributes.label} icon={node.attributes.icon}>
          {node.elements.map((element: any) => reactProcessNode(element))}
        </Menu>
      );
    case "Submenu":
      return (
        <Submenu
          id={node.attributes.id}
          label={node.attributes.label}
          icon={node.attributes.icon}
        >
          {node.elements.map((element: any) => reactProcessNode(element))}
        </Submenu>
      );
    case "Command":
      switch (node.attributes.type) {
        case "FormReferenceMenuItem":
          return (
            <MenuItemForm
              id={node.attributes.id}
              label={node.attributes.label}
              icon={node.attributes.icon}
            />
          );
        case "WorkflowReferenceMenuItem":
          return (
            <MenuItemWorkflow
              id={node.attributes.id}
              label={node.attributes.label}
              icon={node.attributes.icon}
            />
          );
      }
      break;
  }
  console.log(node);
  throw new Error("Unknown menu node.");
}

export function interpretMenu(xmlString: string) {
  const xmlObj = xmlJs.xml2js(xmlString, { compact: false });
  const reactMenu = reactProcessNode(xmlObj);
  return {
    reactMenu
  };
}
