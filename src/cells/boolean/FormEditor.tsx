import * as React from "react";
import { action, observable, runInAction } from "mobx";
import { observer, inject } from "mobx-react";
import { ICellValue, IRecordId, IFieldId } from "../../DataTable/types";
import { IDataCursorState } from "src/Grid/types2";
import { Escape } from "../../utils/keys";
import { IFormEditorProps } from "../types";

@observer
export class BooleanFormEditor extends React.Component<IFormEditorProps> {
  public componentDidMount() {
    runInAction(() => {
      this.dirtyValue = (this.props.value !== undefined
        ? this.props.value
        : false) as boolean;
      this.elmInput!.focus();
      setTimeout(() => {
        this.elmInput && this.elmInput.select();
      }, 10);
    });
  }

  private elmInput: HTMLInputElement | null;

  @observable
  private dirtyValue: boolean = false;
  private isDirty: boolean = false;

  @action.bound
  private refInput(elm: HTMLInputElement) {
    this.elmInput = elm;
  }

  @action.bound
  private handleChange(event: any) {
    this.dirtyValue = event.target.checked;
    this.isDirty = true;
    this.requestDataCommit(
      this.props.editingRecordId!,
      this.props.editingFieldId
    );
  }

  @action.bound
  public requestDataCommit(recordId: string, fieldId: string) {
    if (this.isDirty) {
      console.log("Commit data:", this.dirtyValue);
      this.props.onDataCommit &&
        this.props.onDataCommit(this.dirtyValue, recordId, fieldId);
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
        style={{
          width: "100%",
          height: "100%",
          display: "flex",
          alignContent: "center",
          justifyItems: "center"
        }}
      >
        <input
          onKeyDown={this.handleKeyDown}
          onClick={this.handleClick}
          ref={this.refInput}
          style={{
            margin: "auto",
            border: "none",
            padding: "0px 0px 0px 0px"
          }}
          type="checkbox"
          checked={this.dirtyValue}
          onChange={this.handleChange}
        />
      </div>
    );
  }
}
