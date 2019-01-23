import * as React from "react";
import { action, observable, runInAction } from "mobx";
import { observer, inject } from "mobx-react";
import { ICellValue, IRecordId, IFieldId } from "../../DataTable/types";
import { IDataCursorState } from "src/Grid/types2";
import { Escape } from "../../utils/keys";
import { IFormEditorProps } from "../types";

@observer
export class StringFormEditor extends React.Component<IFormEditorProps> {
  public componentDidMount() {
    runInAction(() => {
      this.dirtyValue = (this.props.value !== undefined
        ? this.props.value
        : "") as string;
      this.elmInput!.focus();
      setTimeout(() => {
        this.elmInput && this.elmInput.select();
      }, 10);
    });
    window.addEventListener("click", this.handleWindowClick);
  }

  public componentWillUnmount() {
    window.removeEventListener("click", this.handleWindowClick);
  }

  @action.bound private handleWindowClick(event: any) {
    if (this.refRoot.current && !this.refRoot.current.contains(event.target)) {
      this.requestDataCommit(
        this.props.editingRecordId!,
        this.props.editingFieldId!
      );
    }
  }

  private elmInput: HTMLInputElement | null;
  private refRoot = React.createRef<HTMLDivElement>();

  @observable
  private dirtyValue: string = "";
  private isDirty: boolean = false;

  @action.bound
  private refInput(elm: HTMLInputElement) {
    this.elmInput = elm;
  }

  @action.bound
  private handleChange(event: any) {
    this.dirtyValue = event.target.value;
    this.isDirty = true;
  }

  @action.bound
  public requestDataCommit(recordId: string, fieldId: string) {
    if (this.isDirty) {
      console.log("Commit data:", this.dirtyValue);
      this.props.onDataCommit &&
        this.props.onDataCommit(this.dirtyValue, recordId, fieldId);
      this.isDirty = false; // TODO: ???
    }
  }

  @action.bound
  private handleKeyDown(event: any) {
    event.stopPropagation();
    switch (event.key) {
      default:
        this.props.onDefaultKeyDown && this.props.onDefaultKeyDown(event);
    }
  }

  @action.bound private handleClick(event: any) {
    event.stopPropagation();
    this.props.onDefaultClick && this.props.onDefaultClick(event);
  }

  public render() {
    return (
      <div
        ref={this.refRoot}
        style={{
          width: "100%",
          height: "100%",
          border: "none",
          padding: 0,
          margin: 0
        }}
      >
        <input
          onKeyDown={this.handleKeyDown}
          onClick={this.handleClick}
          ref={this.refInput}
          style={{
            width: "100%",
            height: "100%",
            border: "none",
            padding: "0px 0px 0px 15px",
            margin: 0
          }}
          value={this.dirtyValue}
          onChange={this.handleChange}
        />
      </div>
    );
  }
}
