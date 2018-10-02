import * as React from "react";
import { action, observable, runInAction } from "mobx";
import { observer } from "mobx-react";
import { ICellValue } from "../../DataTable/types";

@observer
export class StringGridEditor extends React.Component<{
  value: ICellValue | undefined;
  onKeyDown: ((event: any) => void) | undefined;
}> {
  
  public componentDidMount() {
    runInAction(() => {
      this.dirtyValue = this.props.value !== undefined ? this.props.value : '';
      this.elmInput!.focus();
      setTimeout(() => {
        this.elmInput && this.elmInput.select();
      }, 10);
    });
  }

  private elmInput: HTMLInputElement | null;
  @observable
  private dirtyValue: string = "";
  private isDirty: boolean = false;
  private editingCanceled: boolean = false;

  @action.bound
  private refInput(elm: HTMLInputElement) {
    this.elmInput = elm;
  }

  @action.bound
  private handleChange(event: any) {
    this.dirtyValue = event.target.value;
    this.isDirty = true;
  }

  public componentWillUnmount() {
    if (this.isDirty && !this.editingCanceled) {
      console.log("Commit data:", this.dirtyValue);
    }
  }

  @action.bound
  private handleKeyDown(event: any) {
    if(event.key === 'Escape') {
      this.editingCanceled = true;
    }
    this.props.onKeyDown && this.props.onKeyDown(event);
  }

  public render() {
    return (
      <input
        onKeyDown={this.handleKeyDown}
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
    );
  }
}
