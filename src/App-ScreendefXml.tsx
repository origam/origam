import axios from "axios";
import { observable, action } from "mobx";
import { observer, Provider, inject } from "mobx-react";
import * as React from "react";
import * as xmlJs from "xml-js";
import "./App.scss";
import "./styles/screenComponents.scss";
import { isArray } from "util";

function parseScreenDef(o: any) {
  const unhandled: any[] = [];
  const uiStruct: any = { children: [] };

  const DropDownColumns = "DropDownColumns";

  function processNode(node: any, uiStructOpened: any, pathFlags: Set<string>) {
    let newPathFlags: Set<string> = pathFlags;
    let newChild: any = uiStructOpened;
    switch (node.name) {
      case undefined:
        break;
      case "Window":
        newChild = {
          type: node.name,
          props: {
            name: node.attributes.Name,
            id: node.attributes.Id
          },
          children: []
        };
        uiStructOpened.children.push(newChild);
        break;
      case "FormRoot":
        newPathFlags.add("FormRoot");
        newChild = {
          type: "FormRoot",
          props: {},
          children: []
        };
        uiStructOpened.children.push(newChild);
        break;
      case "FormElement": {
        newPathFlags.add("FormElement");
        const location = {
          x: parseInt(node.attributes.X, 10),
          y: parseInt(node.attributes.Y, 10),
          w: parseInt(node.attributes.Width, 10),
          h: parseInt(node.attributes.Height, 10)
        };
        newChild = {
          type: "Panel",
          props: {
            name: node.attributes.Title,
            ...location
          },
          children: []
        };
        uiStructOpened.children.push(newChild);
        break;
      }
      case "string":
        if (pathFlags.has("FormRoot")) {
          newChild = {
            type: "Property",
            props: {
              id: node.elements[0] && node.elements[0].text
            },
            children: []
          };
          uiStructOpened.children.push(newChild);
        }
        break;
      case "UIRoot":
      case "UIElement": {
        const location = {
          x: parseInt(node.attributes.X, 10),
          y: parseInt(node.attributes.Y, 10),
          w: parseInt(node.attributes.Width, 10),
          h: parseInt(node.attributes.Height, 10)
        };
        newChild = {
          type: node.attributes.Type,
          props: {
            name: node.attributes.Name,
            id: node.attributes.Id,
            ...location
          },
          children: []
        };
        if (node.attributes.Type === "Grid") {
          newChild.props.isHeadless = node.attributes.IsHeadless === "true";
        }
        uiStructOpened.children.push(newChild);
        break;
      }
      case "Property": {
        if (pathFlags.has(DropDownColumns)) {
          break;
        }
        const location = {
          x: parseInt(node.attributes.X, 10),
          y: parseInt(node.attributes.Y, 10),
          w: parseInt(node.attributes.Width, 10),
          h: parseInt(node.attributes.Height, 10)
        };
        newChild = {
          type: "Property",
          props: {
            name: node.attributes.Name,
            id: node.attributes.Id,
            entity: node.attributes.Entity,
            ...location,
            captionLength: parseInt(node.attributes.CaptionLength, 10),
            captionPosition: node.attributes.CaptionPosition
          },
          children: []
        };
        uiStructOpened.children.push(newChild);
        break;
      }
      case "DropDownColumns":
        newPathFlags = new Set(newPathFlags.keys());
        newPathFlags.add(DropDownColumns);
        break;
      case "Properties":
        newChild = {
          type: "Properties",
          props: {},
          children: []
        };
        uiStructOpened.children.push(newChild);
        break;
      default:
        unhandled.push(node);
        break;
    }
    if (node.elements) {
      for (const element of node.elements) {
        processNode(element, newChild, newPathFlags);
      }
    }
    if (newChild.type === "Tab") {
      const handles = [];
      for (const child of newChild.children) {
        handles.push({
          type: "TabHandle",
          props: {
            name: child.props.name,
            id: child.props.id
          },
          children: []
        });
      }
      newChild.props.handles = handles;
      if (handles[0]) {
        newChild.props.firstTabId = handles[0].props.id;
      }
    }
  }

  function moveProperties(node: any): any {
    if (node.type === "Grid") {
      const formRoot = node.children.find((ch: any) => ch.type === "FormRoot");
      const properties = node.children.find(
        (ch: any) => ch.type === "Properties"
      );
      const propertiesMap = new Map(
        properties.children.map((prop: any) => [prop.props.id, prop])
      );
      const formRootChildren = [...formRoot.children];
      for (
        let formRootChildIndex = 0;
        formRootChildIndex < formRootChildren.length;
        formRootChildIndex++
      ) {
        const formRootChild = formRootChildren[formRootChildIndex];
        if (formRootChild.type === "Property") {
          formRoot.children[formRootChildIndex] = propertiesMap.get(
            formRootChild.props.id
          );
        } else if (formRootChild.type === "Panel") {
          const panelChildren = [...formRootChild.children];
          for (
            let panelChildIndex = 0;
            panelChildIndex < formRootChild.children.length;
            panelChildIndex++
          ) {
            const panelChild = formRootChild.children[panelChildIndex];
            if (panelChild.type === "Property") {
              formRootChild.children[panelChildIndex] = propertiesMap.get(
                panelChild.props.id
              );
            }
          }
        }
      }
      node.children = node.children.filter(
        (ch: any) => ch.type !== "Properties"
      );
      return node;
    }
    for (const child of node.children) {
      moveProperties(child);
    }
    return node;
  }

  // console.log(o)
  processNode(o, uiStruct, new Set());
  moveProperties(uiStruct);
  // console.log(unhandled);
  return {
    uiStruct
  };
}

class OUIUnknown extends React.Component<any> {
  public render() {
    return (
      <div className="oui-unknown">
        {`Type: ${this.props.type} Name: ${this.props.name}, Id: ${
          this.props.id
        }`}
        {this.props.children}
      </div>
    );
  }
}

class OUIFormRoot extends React.Component<any> {
  public render() {
    return <div className="oui-form-root">{this.props.children}</div>;
  }
}

class OUIProperty extends React.Component<any> {
  public render() {
    if (!Number.isInteger(this.props.x) || !Number.isInteger(this.props.y)) {
      return null;
    }
    let captionLocation;
    if (this.props.captionPosition === "Left") {
      captionLocation = {
        left: this.props.x - this.props.captionLength,
        top: this.props.y,
        width: this.props.captionLength,
        minHeight: 20 // this.props.h,
      };
    } else if (this.props.captionPosition === "Top") {
      captionLocation = {
        left: this.props.x,
        top: this.props.y - 20,
        width: this.props.captionLength,
        minHeight: 20 // this.props.h,
      };
    } else {
      captionLocation = {
        left:
          this.props.x +
          (this.props.entity === "Boolean" ? this.props.h : this.props.w), // + this.props.captionLength,
        top: this.props.y,
        width: this.props.captionLength,
        minHeight: 20 // this.props.h,
      };
    }
    return (
      <>
        <div
          className="oui-property"
          style={{
            top: this.props.y,
            left: this.props.x,
            width:
              this.props.entity === "Boolean" ? this.props.h : this.props.w,
            height: this.props.h
          }}
        >
          {/*`Type: ${this.props.type} Name: ${this.props.name}, Id: ${
            this.props.id
          }`*/}
          {/*this.props.children*/}
          {/*this.props.name*/}
          {this.props.entity}
        </div>
        {this.props.captionPosition !== "None" && (
          <div className="oui-property-caption" style={{ ...captionLocation }}>
            {this.props.name}
          </div>
        )}
      </>
    );
  }
}

class OUIVSplit extends React.Component<any> {
  public render() {
    return (
      <div className="oui-vsplit-container">
        {React.Children.map(this.props.children, child => (
          <div className="oui-vsplit-panel">{child}</div>
        ))}
      </div>
    );
  }
}

class OUIHSplit extends React.Component<any> {
  public render() {
    return (
      <div className="oui-hsplit-container">
        {React.Children.map(this.props.children, child => (
          <div className="oui-hsplit-panel">{child}</div>
        ))}
      </div>
    );
  }
}

class OUIPanel extends React.Component<any> {
  public render() {
    return (
      <>
        <div
          className="oui-panel"
          style={{
            top: this.props.y,
            left: this.props.x,
            width: this.props.w,
            height: this.props.h
          }}
        >
          {this.props.children}
        </div>
        <div
          className="oui-panel-label"
          style={{
            top: this.props.y + 5,
            left: this.props.x + 5
          }}
        >
          {this.props.name}
        </div>
      </>
    );
  }
}

class OUIVBox extends React.Component<any> {
  public render() {
    return (
      <div
        className="oui-vbox"
        style={{
          maxWidth: this.props.w,
          maxHeight: this.props.h
        }}
      >
        {this.props.children}
      </div>
    );
  }
}

class OUIWindow extends React.Component<any> {
  public render() {
    return <div className="oui-window">{this.props.children}</div>;
  }
}

class OUIGrid extends React.Component<any> {
  public render() {
    return (
      <div
        className="oui-grid"
        style={{
          maxWidth: this.props.w,
          maxHeight: this.props.h
        }}
      >
        <div
          className={
            "oui-grid-toolbar" + (this.props.isHeadless ? " hidden" : "")
          }
        >
          tb
        </div>
        {this.props.children}
      </div>
    );
  }
}

@observer
class OUITab extends React.Component<any> {
  @observable
  public activeTabId: string = this.props.firstTabId;

  @action.bound
  public handleHandleClick(event: any, tabId: string) {
    this.activeTabId = tabId;
    console.log("Handle click", this.activeTabId);
  }

  public render() {
    return (
      <Provider tabParent={this}>
        <div className="oui-tab">
          <div className="oui-tab-handles">{this.props.handles}</div>
          <div className="oui-tab-panels">{this.props.children}</div>
        </div>
      </Provider>
    );
  }
}

@inject(stores => {
  const { tabParent } = stores as any;
  const { activeTabId, handleHandleClick } = tabParent;
  return {
    activeTabId,
    onHandleClick: handleHandleClick
  };
})
@observer
class OUITabHandle extends React.Component<any> {
  public render() {
    return (
      <div
        onClick={event => this.props.onHandleClick(event, this.props.id)}
        className={
          "oui-tab-handle" +
          (this.props.id === this.props.activeTabId ? " active" : "")
        }
      >
        {this.props.name}
      </div>
    );
  }
}

@inject(({ tabParent: { activeTabId } }) => {
  return {
    activeTabId
  };
})
@observer
class OUIBox extends React.Component<any> {
  public render() {
    return (
      <div
        className="oui-box"
        style={{
          display: this.props.activeTabId !== this.props.id ? "none" : undefined
        }}
      >
        {this.props.children}
      </div>
    );
  }
}

class OUILabel extends React.Component<any> {
  public render() {
    return <div className="oui-label">{this.props.name}</div>;
  }
}

class OUITreePanel extends React.Component<any> {
  public render() {
    return (
      <div>
        <img
          style={{ width: 250 }}
          src="https://thegraphicsfairy.com/wp-content/uploads/blogger/-GXi8yHjt0fc/T-zV1MX-VfI/AAAAAAAASgI/uChxO5KV9yE/s1600/tree-Vintage-GraphicsFairy6.jpg"
        />
      </div>
    );
  }
}

const elementTypes = {
  FormRoot: OUIFormRoot,
  Property: OUIProperty,
  Panel: OUIPanel,
  VSplit: OUIVSplit,
  HSplit: OUIHSplit,
  Window: OUIWindow,
  Grid: OUIGrid,
  Tab: OUITab,
  Box: OUIBox,
  VBox: OUIVBox,
  TabHandle: OUITabHandle,
  Label: OUILabel,
  TreePanel: OUITreePanel
};

function createUITree(uiStruct: any): any {
  if (isArray(uiStruct)) {
    console.log(uiStruct);
  }

  if (isArray(uiStruct)) {
    return uiStruct.map(uiStr => createUITree(uiStr));
  }
  const elementClass = elementTypes[uiStruct.type];
  if (!elementClass) {
    return React.createElement(
      OUIUnknown,
      { ...uiStruct.props, type: uiStruct.type },
      uiStruct.children.map((child: any) => createUITree(child))
    );
  } else {
    switch (uiStruct.type) {
      case "Tab":
        return React.createElement(
          elementClass,
          {
            ...uiStruct.props,
            handles: uiStruct.props.handles.map((child: any) =>
              createUITree(child)
            )
          },
          uiStruct.children.map((child: any) => createUITree(child))
        );
        break;
      default:
        return React.createElement(
          elementClass,
          uiStruct.props,
          uiStruct.children.map((child: any) => createUITree(child))
        );
        break;
    }
  }
}

@observer
class App extends React.Component {
  @observable.ref
  public xmlObj: any;
  @observable.ref
  public screenDef: any;

  public async componentDidMount() {
    const xml = (await axios.get("/screen02.xml")).data;
    this.xmlObj = xmlJs.xml2js(xml, { compact: false });

    const xo = this.xmlObj;
    const screenDef = parseScreenDef(xo);
    this.screenDef = screenDef;
    console.log(this.screenDef.uiStruct.children[0]);
  }

  public render() {
    return (
      <>
        {/**/}

        {this.screenDef && createUITree(this.screenDef.uiStruct.children[0])}
        {/*<pre>{JSON.stringify(this.xmlObj, null, 2)}</pre>*/}
        {/*<pre>
          {JSON.stringify(
            this.screenDef && this.screenDef.uiStruct.children[0],
            null,
            2
          )}
        </pre>*/}
      </>
    );
  }
}

export default App;
