
import { buildReactTree } from "./uiBuilder";
import { GridViewType } from "src/Grid/types";

function getLocation(node: any): any {
  return {
    x: parseInt(node.attributes.X, 10) || undefined,
    y: parseInt(node.attributes.Y, 10) || undefined,
    w: parseInt(node.attributes.Width, 10) || undefined,
    h: parseInt(node.attributes.Height, 10) || undefined
  };
}

const BASIC_RULES = [
  ruleWindow,
  ruleUIRoot,
  ruleUIChildren,
  ruleChildren,
  ruleFormElement,
  ruleProperties,
  ruleDropdownColumns,
  ruleUIElement,
  ruleFormRoot,
  ruleDataSources,
  ruleComponentBindings,
  ruleConfiguration,
  rulePropertyNames,
  ruleUnknownWarn
];

const UI_ELEMENT_RULES = [
  ruleVBox,
  ruleTab,
  ruleBox,
  ruleVSplit,
  ruleHSplit,
  ruleLabel,
  ruleGrid,
  ruleFormSection,
  ruleTreePanel,
  ruleUnknownWarn
];

function ruleRoot(node: any, context: any, rules: any[]) {
  node.elements.forEach((element: any) =>
    processNode(element, context, BASIC_RULES)
  );
  return node;
}

function ruleFormRoot(node: any, context: any, rules: any[]) {
  if (node.name === "FormRoot") {
    node.elements.forEach((element: any) => {
      processNode(element, context, BASIC_RULES);
    });
    return node;
  }
  return undefined;
}

function ruleWindow(node: any, context: any, rules: any[]) {
  if (node.name === "Window") {
    const uiNode = {
      type: node.name,
      props: {
        name: node.attributes.Title,
        id: node.attributes.Id
      },
      children: []
    };
    const newContext = { ...context, uiNode };
    node.elements &&
      node.elements.forEach((element: any) =>
        processNode(element, newContext, BASIC_RULES)
      );
    context.uiNode = uiNode;
    return node;
  }
  return undefined;
}

function ruleProperty(node: any, context: any, rules: any[]) {
  if (node.name === "Property") {
    const collectDropdownColumns: any[] = [];
    context.collectProperties.push({
      id: node.attributes.Id,
      name: node.attributes.Name,
      entity: node.attributes.Entity,
      column: node.attributes.Column,
      x: parseInt(node.attributes.X, 10),
      y: parseInt(node.attributes.Y, 10),
      w: parseInt(node.attributes.Width, 10),
      h: parseInt(node.attributes.Height, 10),
      captionLength: parseInt(node.attributes.CaptionLength, 10),
      captionPosition: node.attributes.CaptionPosition,
      lookupId: node.attributes.LookupId,
      lookupIdentifier: node.attributes.Identifier,
      dropdownColumns: collectDropdownColumns,
    });
    const newContext = {
      ...context,
      collectDropdownColumns
    };
    node.elements && node.elements.forEach((element: any) => {
      processNode(element, newContext, BASIC_RULES);
    });
    return node;
  }
  return undefined;
}

function ruleDropdownColumns(node: any, context: any, rules: any[]) {
  if(node.name === "DropDownColumns") {
    node.elements.forEach((element: any) => {
      processNode(element, context, [ruleDropdownColumn]);
    })
    return node;
  }
  return undefined;
}

function ruleDropdownColumn(node: any, context: any, rules: any[]) {
  if(node.name === "Property") {
    context.collectDropdownColumns.push({
      id: node.attributes.Id,
      name: node.attributes.Name,
      entity: node.attributes.Entity,
      column: node.attributes.Column
    })
    return node;
  }
  return undefined;
}

function ruleProperties(node: any, context: any, rules: any[]) {
  if (node.name === "Properties") {
    node.elements.forEach((element: any) => {
      processNode(element, context, [ruleProperty]);
    });
    return node;
  }
  return undefined;
}

function ruleStringPropertyName(node: any, context: any, rules: any[]) {
  if (node.name === "string") {
    const propertyTarget =
      context.collectFormChildren || context.uiNode.children;
    context.executeLater.push(() => {
      const foundProperty = context.collectProperties.find((property: any) => {
        return property.id === (node.elements[0] && node.elements[0].text);
      });
      if (foundProperty) {
        propertyTarget.push({
          type: "FormField",
          props: {
            property: foundProperty
          },
          children: []
        });
      }
    });
    return node;
  }
  return undefined;
}

function rulePropertyNames(node: any, context: any, rules: any[]) {
  if (node.name === "PropertyNames") {
    node.elements.forEach((element: any) => [
      processNode(element, context, [ruleStringPropertyName])
    ]);
    return node;
  }
  return undefined;
}

function ruleUIRoot(node: any, context: any, rules: any[]) {
  if (node.name === "UIRoot") {
    processNode(node, context, UI_ELEMENT_RULES);
    return node;
  }
  return undefined;
}

function ruleUIChildren(node: any, context: any, rules: any[]) {
  if (node.name === "UIChildren") {
    node.elements.forEach((element: any) => {
      processNode(element, context, BASIC_RULES);
    });
    return node;
  }
  return undefined;
}

function ruleChildren(node: any, context: any, rules: any[]) {
  if (node.name === "Children") {
    node.elements.forEach((element: any) => {
      processNode(element, context, BASIC_RULES);
    });
    return node;
  }
  return undefined;
}

function ruleFormElement(node: any, context: any, rules: any[]) {
  if (node.name === "FormElement") {
    processNode(node, context, UI_ELEMENT_RULES);
    return node;
  }
  return undefined;
}

function ruleUIElement(node: any, context: any, rules: any[]) {
  if (node.name === "UIElement") {
    processNode(node, context, UI_ELEMENT_RULES);
    return node;
  }
  return undefined;
}

function ruleTreePanel(node: any, context: any, rules: any[]) {
  if (node.attributes.Type === "TreePanel") {
    const uiNode = {
      type: node.attributes.Type,
      props: {
        id: node.attributes.Id,
        name: node.attributes.Name,
        ...getLocation(node)
      },
      children: []
    };
    context.uiNode.children.push(uiNode);

    const newContext = {
      ...context,
      uiNode
    };
    node.elements.forEach((element: any) => {
      processNode(element, newContext, BASIC_RULES);
    });
    return node;
  }
  return undefined;
}

function text2bool(t: string) {
  if (t === "true") {
    return true;
  }
  if (t === "false") {
    return false;
  }
  throw new Error(`Unknown boolean variable ${t}`);
}



function ruleGrid(node: any, context: any, rules: any[]) {
  if (node.attributes.Type === "Grid") {
    // console.log(node.attributes);
    const settings = {
      entity: node.attributes.Entity,
      isHeadless: text2bool(node.attributes.IsHeadless),
      isActionButtonsDisabled: text2bool(node.attributes.DisableActionButtons),
      isShowAddButton: text2bool(node.attributes.ShowAddButton),
      isShowDeleteButton: text2bool(node.attributes.ShowDeleteButton),
      initialView: [
        GridViewType.Form,
        GridViewType.Grid,
        null,
        null,
        null,
        GridViewType.Map
      ][parseInt(node.attributes.DefaultPanelView, 10)]
    };
    const collectFormChildren: React.ReactNode[] = [];
    const collectTableChildren: React.ReactNode[] = [];
    const uiNode = {
      type: node.attributes.Type,
      props: {
        id: node.attributes.Id,
        name: node.attributes.Name,
        ...settings,
        form: [
          {
            type: "Form",
            props: {},
            children: collectFormChildren
          }
        ],
        table: [
          {
            type: "Table",
            props: {},
            children: collectTableChildren
          }
        ],
        properties: [],
        ...getLocation(node)
      },
      children: []
    };
    context.uiNode.children.push(uiNode);

    const newContext = {
      ...context,
      collectFormChildren,
      collectTableChildren,
      collectProperties: uiNode.props.properties,
      uiNode
    };
    node.elements.forEach((element: any) => {
      processNode(element, newContext, BASIC_RULES);
    });

    newContext.executeLater.push(() => {
      const dataSource = context.dataSources.find((ds: any) => {
        return ds.entity === settings.entity;
      });
      if(dataSource) {
        uiNode.props.dataSource = dataSource;
        for(const property of newContext.collectProperties) {
          const field = dataSource.fields.find((f: any) => f.id === property.id)
          if(field) {
            property.recvDataIndex = field.recvDataIndex;
            property.isPrimaryKey = dataSource.primaryKey === field.id;
          } else {
            // TODO: No field found?
          }
          console.log(property);
        }
        
      } else {
        // TODO: No data source found?
      }
    })

    return node;
  }
  return undefined;
}

function ruleVSplit(node: any, context: any, rules: any[]) {
  if (node.attributes.Type === "VSplit") {
    const uiNode = {
      type: node.attributes.Type,
      props: {
        name: node.attributes.Name,
        id: node.attributes.Id,
        ...getLocation(node)
      },
      children: []
    };
    context.uiNode.children.push(uiNode);

    const newContext = { ...context, uiNode };
    node.elements.forEach((element: any) => {
      processNode(element, newContext, BASIC_RULES);
    });

    return node;
  }
  return undefined;
}

function ruleHSplit(node: any, context: any, rules: any[]) {
  if (node.attributes.Type === "HSplit") {
    const uiNode = {
      type: node.attributes.Type,
      props: {
        name: node.attributes.Name,
        id: node.attributes.Id,
        ...getLocation(node)
      },
      children: []
    };
    context.uiNode.children.push(uiNode);

    const newContext = { ...context, uiNode };
    node.elements.forEach((element: any) => {
      processNode(element, newContext, BASIC_RULES);
    });

    return node;
  }
  return undefined;
}

function ruleVBox(node: any, context: any, rules: any[]) {
  if (node.attributes.Type === "VBox") {
    const uiNode = {
      type: node.attributes.Type,
      props: {
        name: node.attributes.Name,
        id: node.attributes.Id,
        ...getLocation(node)
      },
      children: []
    };
    context.uiNode.children.push(uiNode);

    const newContext = { ...context, uiNode };
    node.elements.forEach((element: any) => {
      processNode(element, newContext, BASIC_RULES);
    });

    return node;
  }
  return undefined;
}

function ruleLabel(node: any, context: any, rules: any[]) {
  if (node.attributes.Type === "Label") {
    const uiNode = {
      type: node.attributes.Type,
      props: {
        name: node.attributes.Name,
        id: node.attributes.Id,
        ...getLocation(node)
      },
      children: []
    };
    context.uiNode.children.push(uiNode);

    const newContext = { ...context, uiNode };
    node.elements.forEach((element: any) => {
      processNode(element, newContext, BASIC_RULES);
    });

    return node;
  }
  return undefined;
}

function ruleFormSection(node: any, context: any, rules: any[]) {
  if (node.attributes.Type === "FormSection") {
    const uiNode = {
      type: node.attributes.Type,
      props: {
        name: node.attributes.Title,
        id: node.attributes.Id,
        ...getLocation(node)
      },
      children: []
    };
    if (context.collectFormChildren) {
      context.collectFormChildren.push(uiNode);
    } else {
      context.uiNode.children.push(uiNode);
    }

    const newContext = { ...context, uiNode };
    delete newContext.collectFormChildren;
    node.elements.forEach((element: any) => {
      processNode(element, newContext, BASIC_RULES);
    });

    return node;
  }
  return undefined;
}

function ruleBox(node: any, context: any, rules: any[]) {
  if (node.attributes.Type === "Box") {
    const uiNode = {
      type: node.attributes.Type,
      props: {
        name: node.attributes.Name,
        id: node.attributes.Id,
        ...getLocation(node)
      },
      children: []
    };
    context.uiNode.children.push(uiNode);

    if (context.uiNode.type === "Tab") {
      context.uiNode.props.handles.push({
        type: "TabHandle",
        props: {
          name: node.attributes.Name,
          id: node.attributes.Id
        },
        children: []
      });
    }
    const newContext = { ...context, uiNode };
    node.elements.forEach((element: any) => {
      processNode(element, newContext, BASIC_RULES);
    });

    return node;
  }
  return undefined;
}

function ruleTab(node: any, context: any, rules: any[]) {
  if (node.attributes.Type === "Tab") {
    const uiNode = {
      type: node.attributes.Type,
      props: {
        name: node.attributes.Name,
        id: node.attributes.Id,
        handles: [],
        ...getLocation(node)
      },
      children: []
    };
    context.uiNode.children.push(uiNode);

    const newContext = { ...context, uiNode };
    node.elements.forEach((element: any) => {
      processNode(element, newContext, BASIC_RULES);
    });
    if (uiNode.props.handles[0]) {
      uiNode.props.firstTabId = uiNode.props.handles[0].props.id;
    }

    return node;
  }
  return undefined;
}

function ruleDataSources(node: any, context: any, rules: any[]) {
  if (node.name === "DataSources") {
    const dataSources = [];
    for(const element1 of node.elements) {
      if(element1.name === "DataSource") {
        const fields = [];
        for(const element2 of element1.elements) {
          if(element2.name === "Field") {
            fields.push({
              id: element2.attributes.Name,
              recvDataIndex: parseInt(element2.attributes.Index, 10),
            });
          }
        }
        dataSources.push({
          entity: element1.attributes.Entity,
          dataStructureEntityId: element1.attributes.DataStructureEntityId,
          primaryKey: element1.attributes.Identifier,
          lookupCacheKey: element1.attributes.LookupCacheKey,
          fields
        })
      }
      context.dataSources = dataSources;
      console.log(dataSources)
    }
    return node;
  }
  return undefined;
}



function ruleComponentBindings(node: any, context: any, rules: any[]) {
  if (node.name === "ComponentBindings") {
    console.warn(`No processing for ${node.name} so far.`);
    return node;
  }
  return undefined;
}

function ruleConfiguration(node: any, context: any, rules: any[]) {
  if (node.name === "Configuration") {
    console.warn(`No processing for ${node.name} so far.`);
    return node;
  }
  return undefined;
}

function ruleUnknownWarn(node: any, context: any) {
  // console.log(node, context);
  console.warn(
    `Unknown node ${node.name} ${node.attributes &&
      node.attributes.Id} ${node.attributes && node.attributes.Type}`
  );
  return node;
}

export function processNode(node: any, context: any, rules: any[]) {
  for (const rule of rules) {
    const ruleResult = rule(node, context, rules);
    if (ruleResult !== undefined) {
      return ruleResult;
    }
  }
  throw new Error("No rulle triggered.");
}

export function parseScreenDef(o: any) {
  const context = {
    uiNode: null,
    executeLater: []
  };
  processNode(o, context, [ruleRoot, ruleUnknownWarn]);
  context.executeLater.forEach((run: any) => run());
  context.executeLater = [];
  return context;
}

export async function main() {
  return;
}
