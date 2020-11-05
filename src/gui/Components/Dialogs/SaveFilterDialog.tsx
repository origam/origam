import { observer } from "mobx-react";
import React from "react";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
import { T } from "utils/translation";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import S from "gui/Components/Dialogs/SaveFilterDialog.module.css";
import { observable } from "mobx";
import {MultiGrid} from "react-virtualized";

@observer
export class SaveFilterDialog extends React.Component<{
  onCancelClick: (event: any) => void;
  onOkClick: (name: string, isGlobal: boolean) => void;
}> {

  @observable
  filterName: string = "";

  @observable
  isGlobal: boolean = false;

  refInput = React.createRef<HTMLInputElement>();

  onNameChanged(event: any){
    this.filterName = event.target.value;
  }

  onIsGlobalClicked(event: any){
    this.isGlobal = event.target.checked;
  }

  componentDidMount() {
    this.refInput.current?.focus();
  }

  onKeydown(event: React.KeyboardEvent<HTMLInputElement>){
    if(event.key === "Enter"){
      this.props.onOkClick(this.filterName, this.isGlobal);
    }
  }

  render() {
    return (
      <ModalWindow
        title={T("New Filter", "new_filter_title")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button onClick={() => this.props.onOkClick(this.filterName, this.isGlobal)}>
              {T("Ok", "button_ok")}
            </button>
            <button onClick={this.props.onCancelClick}>
              {T("Cancel", "button_cancel")}
            </button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={CS.dialogContent}>
          <div className={S.inpuContainer}>
            <div className={S.row}>
              <div className={S.label}>
                {T("Name:", "new_filter_name")}
              </div>
              <input
                ref={this.refInput}
                className={S.textInput}
                value={this.filterName}
                onChange={event => this.onNameChanged(event)}
                onKeyDown={(event: React.KeyboardEvent<HTMLInputElement>) => this.onKeydown(event)}
              />
            </div>
            <div className={S.row}>
              <div className={S.label}>
                {T("Global:", "new_filter_global")}
              </div>
              <input
                className={S.chcekBoxinput}
                type="checkbox"
                checked={this.isGlobal}
                onChange={event => this.onIsGlobalClicked(event)}
              />
            </div>
          </div>
        </div>
      </ModalWindow>
    );
  }
}