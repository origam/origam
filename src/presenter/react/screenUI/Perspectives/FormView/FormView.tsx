import * as React from "react";

import { Provider, observer, inject } from "mobx-react";
import {
  IFormField,
  ICaptionPosition,
  IUIFormViewTreeNode,
  IFormView
} from "../../../../view/Perspectives/FormView/types";
import { TextEditor } from "../editors/Text";
import { DateTimeEditor } from "../editors/DateTime";
import { BoolEditor } from "../editors/Bool";
import { Toolbar } from "../DefaultToolbar/Toolbar";
import { parseNumber } from "../../../../../utils/xml";
import { FormViewPresenter } from "../../../../view/Perspectives/FormView/FormViewPresenter";
import { FormViewToolbar } from "../../../../view/Perspectives/FormView/FormViewToolbar";
import { IFormViewMediator } from "../../../../../DataView/FormView/FormViewMediator";
import { action } from "mobx";
import { DropdownEditor } from "../editors/Dropdown";

@inject(({ formViewPresenter }) => {
  return {
    formViewPresenter
  };
})
@observer
export class FormRoot extends React.Component<{
  formViewPresenter?: IFormView;
}> {
  componentDidMount() {
    window.addEventListener("click", this.handleWindowClick);
  }

  componentWillUnmount() {
    window.removeEventListener("click", this.handleWindowClick);
  }

  @action.bound handleWindowClick(event: any) {
    if (this.elmFormRoot && !this.elmFormRoot.contains(event.target)) {
      this.props.formViewPresenter!.onOutsideFormClick &&
        this.props.formViewPresenter!.onOutsideFormClick(event);
    }
  }

  elmFormRoot: HTMLDivElement | null = null;
  refFormRoot = (elm: HTMLDivElement | null) => (this.elmFormRoot = elm);

  render() {
    return (
      <div
        ref={this.refFormRoot}
        className="form-root"
        onClick={this.props.formViewPresenter!.onNoFieldClick}
      >
        {this.props.children}
      </div>
    );
  }
}

@inject(({ formViewPresenter }) => {
  return { listenForRefocus: formViewPresenter.aFocusEditor.listenForRefocus };
})
@observer
export class Editor extends React.Component<{
  field: IFormField;
  listenForRefocus?: (cb: () => void) => () => void;
}> {
  getEditor(field: IFormField) {
    switch (field.type) {
      case "TextCell":
        return (
          <TextEditor
            value={field.value}
            isReadOnly={field.isReadOnly}
            isInvalid={field.isInvalid}
            isFocused={field.isFocused}
            refocuser={this.props.listenForRefocus!}
            onChange={field.onChange}
            onKeyDown={field.onKeyDown}
            onClick={field.onClick}
          />
        );
      case "DateTimeCell":
        return (
          <DateTimeEditor
            value={field.value}
            inputFormat={field.inputFormat}
            outputFormat={field.outputFormat}
            isReadOnly={field.isReadOnly}
            isInvalid={field.isInvalid}
            isFocused={field.isFocused}
          />
        );
      case "BoolCell":
        return (
          <BoolEditor
            value={field.value}
            isReadOnly={field.isReadOnly}
            onChange={field.onChange}
            onClick={field.onClick}
            onKeyDown={field.onKeyDown}
          />
        );
      case "DropdownCell":
        return (
          <DropdownEditor
            value={field.value}
            textualValue={field.textualValue}
            isReadOnly={field.isReadOnly}
            isInvalid={field.isInvalid}
            isFocused={field.isFocused}
            onTextChange={field.onTextChange}
            onItemSelect={field.onItemSelect}
            DataStructureEntityId={field.DataStructureEntityId}
            ColumnNames={field.ColumnNames}
            Property={field.Property}
            RowId={field.RowId}
            LookupId={field.LookupId}
            menuItemId={field.menuItemId}
            api={field.api}
          />
        );
      default:
        return field.value;
    }
  }

  render() {
    return this.getEditor(this.props.field);
  }
}

@observer
export class FormSection extends React.Component<{
  Width: string;
  Height: string;
  Y: string;
  X: string;
  Title: string;
}> {
  render() {
    return (
      <div
        className="form-section"
        style={{
          top: parseNumber(this.props.Y),
          left: parseNumber(this.props.X),
          width: parseNumber(this.props.Width),
          height: parseNumber(this.props.Height)
        }}
      >
        <div className="section-title">{this.props.Title}</div>
        {this.props.children}
      </div>
    );
  }
}

@inject(({ formViewPresenter }) => {
  return { formFields: formViewPresenter.fields };
})
@observer
export class FormField extends React.Component<{
  Id: string;
  Name: string;
  CaptionLength: string;
  CaptionPosition: ICaptionPosition;
  Column: string;
  Entity: string;
  Height: string;
  Width: string;
  Y: string;
  X: string;
  formFields?: Map<string, IFormField>;
}> {
  fieldNameStyle() {
    // TODO: !!! Proper typing of props (numbers instead of strings etc...)
    switch (this.props.CaptionPosition) {
      case ICaptionPosition.Left:
        return {
          top: parseNumber(this.props.Y),
          left:
            parseNumber(this.props.X) - parseNumber(this.props.CaptionLength),
          width: parseNumber(this.props.CaptionLength)
          //  height: this.props.height
        };
      case ICaptionPosition.Right:
        return {
          top: parseNumber(this.props.Y),
          left:
            this.props.Column === "CheckBox"
              ? parseNumber(this.props.X) + parseNumber(this.props.Height)
              : parseNumber(this.props.X) + parseNumber(this.props.Width),
          width: parseNumber(this.props.CaptionLength)
          //  height: this.props.height
        };
      case ICaptionPosition.Top:
        return {
          top: parseNumber(this.props.Y) - 20,
          left: parseNumber(this.props.X),
          width: parseNumber(this.props.CaptionLength)
        };
      default:
        return {};
    }
  }

  render() {
    const field = this.props.formFields!.get(this.props.Id);
    return (
      <>
        {this.props.CaptionPosition !== ICaptionPosition.None && (
          <div className="form-field-name" style={this.fieldNameStyle()}>
            {this.props.Name}
          </div>
        )}
        <div
          className="form-field"
          style={{
            top: parseNumber(this.props.Y),
            left: parseNumber(this.props.X),
            width: parseNumber(this.props.Width),
            height: parseNumber(this.props.Height)
          }}
          onClick={(event: any) => event.stopPropagation()}
        >
          {field ? (
            <Editor field={field} />
          ) : (
            <div className="unknown-editor">{this.props.Id}</div>
          )}
        </div>
      </>
    );
  }
}

@observer
export class FormView extends React.Component<{
  controller: IFormViewMediator;
}> {
  constructor(props: any) {
    super(props);
    this.formViewPresenter = new FormViewPresenter({
      toolbar: () => toolbar,
      mediator: this.props.controller,
      uiStructure: () => this.props.controller.uiStructure,
      propReorder: () => this.props.controller.propReorder,
      dataTable: () => this.props.controller.dataView.dataTable,
      recCursor: () => this.props.controller.dataView.recCursor,
      propCursor: () => this.props.controller.propCursor,
      form: () => this.props.controller.form,
      aSelPrevProp: () => this.props.controller.aSelPrevProp,
      aSelNextProp: () => this.props.controller.aSelNextProp,
      aSelProp: () => this.props.controller.aSelProp,
      isLoading: () => this.props.controller.dataView.machine.isLoading,
      aFocusEditor: this.props.controller.aFocusEditor,
      api: () => this.props.controller.dataView.api,
      dataViewMediator: () => this.props.controller.dataView
    });
    const toolbar = this.props.controller.dataView.isHeadless
      ? undefined
      : new FormViewToolbar({
          aSwitchView: () => this.props.controller.dataView.aSwitchView,
          mediator: () => this.props.controller,
          label: this.props.controller.dataView.label,
          isLoading: () => this.props.controller.dataView.machine.isLoading,
          selection: this.props.controller.selection,
          dataTable: this.props.controller.dataTable
        });
  }

  formViewPresenter: FormViewPresenter;

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
        case "Property":
          return (
            <FormField key={keyGen++} {...node.props}>
              {node.children.map((child: IUIFormViewTreeNode) =>
                recursive(child)
              )}
            </FormField>
          );
      }
    }
    return this.props.controller.uiStructure.map(n => recursive(n));
  }

  render() {
    return (
      <div className="form-view">
        {this.formViewPresenter.toolbar && (
          <Toolbar controller={this.formViewPresenter.toolbar} />
        )}
        <Provider formViewPresenter={this.formViewPresenter}>
          <>{this.buildForm()}</>
        </Provider>
      </div>
    );
  }
}
