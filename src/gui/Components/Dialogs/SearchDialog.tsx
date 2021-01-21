import { Icon } from "gui02/components/Icon/Icon";
import { observer } from "mobx-react";
import React from "react";
import { T } from "utils/translation";
import { ModalWindow } from "../Dialog/Dialog";
import S from "gui/Components/Dialogs/SearchDialog.module.scss";
import { observable } from "mobx";

@observer
export class SearchDialog extends React.Component<{
  onCloseClick: () => void;
}> {

  onKeyDown(event: any){
    if(event.key === "Escape"){
      this.props.onCloseClick();
    }
  }

  render() {
    return (
      <ModalWindow
        title={null}
        titleButtons={null}
        buttonsCenter={
          <>
            <button tabIndex={0} onClick={() => this.props.onCloseClick()}>
              {T("Close", "button_close")}
            </button>
          </>
        }
        onKeyDown={(event:any) => this.onKeyDown(event)}
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={S.root}>
          <div className={S.inputRow}>
            <Icon className={S.icon} src="./icons/search.svg" />
            <input
              className={S.input}
              placeholder="Search for anything here!"
            />
          </div>
          <div>
            <ResultGroup name={"Menu"} items={["bla", "ble", "blu"]} />
            <ResultGroup name={"Contact"} items={["bla", "ble", "blu"]} />
          </div>
        </div>
      </ModalWindow>
    );
  }
}

@observer
export class ResultGroup extends React.Component<{
  name: string;
  items: string[];
}> {
  @observable
  isExpanded = false;

  onGroupClick() {
    this.isExpanded = !this.isExpanded;
  }

  render() {
    return (
      <div>
        <div className={S.resultGroupRow} onClick={() => this.onGroupClick()}>
          {this.isExpanded ? (
            <i className={"fas fa-angle-up " + S.arrow} />
          ) : (
            <i className={"fas fa-angle-down " + S.arrow} />
          )}
          <div className={S.groupName}>
            {this.props.name}
          </div>
        </div>
        <div>
          {this.isExpanded && this.props.items.map(item => <ResultItem name={item}/> )}
        </div>
      </div>
    );
  }
}



@observer
export class ResultItem extends React.Component<{
  name: string;
}> {

  render() {
    return (
      <div className={S.resultIemRow} >
        <div className={S.itemIcon}>
          <Icon src="./icons/document.svg" />
        </div>
        <div className={S.itemContents}>
          <div className={S.itemTitle}>
            {this.props.name}
          </div>
          <div className={S.itemTextSeparator}>
            {" "}
          </div>
          <div className={S.itemText}>
            {"text"}
          </div>
        </div>
      </div>
    );
  }
}

