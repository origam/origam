import axios from "axios";
import { observable, action } from "mobx";
import { observer, Provider, inject } from "mobx-react";
import * as React from "react";
import * as xmlJs from "xml-js";
import "./App.css";
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
        console.log(`${node.name} (Type=${node.attributes.Type})`);
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
      case "UIRoot":
      case "UIElement":
        console.log(`${node.name} (Type=${node.attributes.Type})`);
        newChild = {
          type: node.attributes.Type,
          props: {
            name: node.attributes.Name,
            id: node.attributes.Id
          },
          children: []
        };
        uiStructOpened.children.push(newChild);
        break;
      case "Property":
        if (pathFlags.has(DropDownColumns)) {
          break;
        }
        console.log(
          `${node.name} (Entity=${node.attributes.Entity},Name=${
            node.attributes.Name
          })`
        );
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

  // console.log(o)
  processNode(o, uiStruct, new Set());
  console.log(JSON.stringify(uiStruct, null, 2));
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

class OUIProperties extends React.Component<any> {
  public render() {
    return (
      <div className="oui-properties-container">{this.props.children}</div>
    );
  }
}

class OUIProperty extends React.Component<any> {
  public render() {
    console.log(this.props.x);
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
        <div className="oui-property-caption" style={{ ...captionLocation }}>
          {this.props.name}
        </div>
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

class OUIVBox extends React.Component<any> {
  public render() {
    return <div className="oui-vbox">{this.props.children}</div>;
  }
}

class OUIWindow extends React.Component<any> {
  public render() {
    return <div className="oui-window">{this.props.children}</div>;
  }
}

class OUIGrid extends React.Component<any> {
  public render() {
    return <div className="oui-grid">{this.props.children}</div>;
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
        className="oui-tab-handle"
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

const elementTypes = {
  Properties: OUIProperties,
  Property: OUIProperty,
  VSplit: OUIVSplit,
  HSplit: OUIHSplit,
  Window: OUIWindow,
  Grid: OUIGrid,
  Tab: OUITab,
  Box: OUIBox,
  VBox: OUIVBox,
  TabHandle: OUITabHandle,
  Label: OUILabel
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
        <pre>{JSON.stringify(this.xmlObj, null, 2)}</pre>
      </>
    );
  }
}

export default App;
