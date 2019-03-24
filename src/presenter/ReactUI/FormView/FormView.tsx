import * as React from "react";
import { Toolbar } from "../controls/Toolbar/Toolbar";
import { Provider, observer, inject } from "mobx-react";
import { Editor } from "../DataView/Editor";
import { ICaptionPosition } from "src/presenter/types/IFormViewPresenter/ICaptionPosition";
import { IFormField } from "src/presenter/types/IFormViewPresenter/IFormField";
import {
  IUIFormRoot,
  IUIFormSection,
  IUIFormField
} from "src/presenter/types/IUIScreenBlueprints";
import { IFormView } from "src/presenter/types/IFormViewPresenter/IFormView";

export class FormRoot extends React.Component {
  render() {
    return <div className="form-root">{this.props.children}</div>;
  }
}

export class FormSection extends React.Component<{
  width: number;
  height: number;
  top: number;
  left: number;
  title: string;
}> {
  render() {
    return (
      <div
        className="form-section"
        style={{
          top: this.props.top,
          left: this.props.left,
          width: this.props.width,
          height: this.props.height
        }}
      >
        <div className="section-title">{this.props.title}</div>
        {this.props.children}
      </div>
    );
  }
}

@inject(({ formFields }) => {
  return { formFields };
})
@observer
export class FormField extends React.Component<{
  id: string;
  name?: string;
  captionLength?: number;
  captionPosition?: ICaptionPosition;
  column?: string;
  entity?: string;
  height?: number;
  width?: number;
  top?: number;
  left?: number;
  formFields?: Map<string, IFormField>;
}> {
  fieldNameStyle() {
    // TODO: Hidden fields?
    if (
      this.props.width === undefined ||
      this.props.height === undefined ||
      this.props.top === undefined ||
      this.props.left === undefined
    ) {
      return {};
    }
    switch (this.props.captionPosition) {
      case ICaptionPosition.Left:
        return {
          top: this.props.top,
          left: this.props.left - (this.props.captionLength || 0),
          width: this.props.captionLength
          //  height: this.props.height
        };
      case ICaptionPosition.Right:
        return {
          top: this.props.top,
          left:
            this.props.column === "CheckBox"
              ? this.props.left + (this.props.height || 0)
              : this.props.left + (this.props.width || 0),
          width: this.props.captionLength
          //  height: this.props.height
        };
      case ICaptionPosition.Top:
        return {
          top: this.props.top - 20,
          left: this.props.left,
          width: this.props.captionLength
        };
      default:
        return {};
    }
  }

  render() {
    const field = this.props.formFields!.get(this.props.id);
    return (
      <>
        {this.props.captionPosition !== ICaptionPosition.None && (
          <div className="form-field-name" style={this.fieldNameStyle()}>
            {this.props.name}
          </div>
        )}
        <div
          className="form-field"
          style={{
            top: this.props.top,
            left: this.props.left,
            width: this.props.width,
            height: this.props.height
          }}
        >
          {field ? (
            <Editor field={field} />
          ) : (
            <div className="unknown-editor">{this.props.id}</div>
          )}
        </div>
      </>
    );
  }
}

type IUIFormViewTreeNode = IUIFormRoot | IUIFormSection | IUIFormField;

export class FormView extends React.Component<{ controller: IFormView }> {
  buildForm() {
    let keyGen = 0;
    function recursive(node: IUIFormViewTreeNode) {
      switch (node.type) {
        case "FormRoot":
          return (
            <FormRoot key={keyGen++}>
              {node.children.map((child: IUIFormViewTreeNode) =>
                recursive(child)
              )}
            </FormRoot>
          );
        case "FormSection":
          return (
            <FormSection key={keyGen++} {...node.props}>
              {node.children.map((child: IUIFormViewTreeNode) =>
                recursive(child)
              )}
            </FormSection>
          );
        case "FormField":
          return (
            <FormField key={keyGen++} {...node.props}>
              {node.children.map((child: IUIFormViewTreeNode) =>
                recursive(child)
              )}
            </FormField>
          );
      }
    }

    return this.props.controller.uiStructure.map(node => recursive(node));
  }

  render() {
    return (
      <div className="form-view">
        {this.props.controller.toolbar && (
          <Toolbar controller={this.props.controller.toolbar} />
        )}
        <Provider formFields={this.props.controller.fields}>
          <>{this.buildForm()}</>
        </Provider>
      </div>
    );
  }
}
