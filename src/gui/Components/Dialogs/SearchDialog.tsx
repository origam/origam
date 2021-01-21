import { Icon } from "gui02/components/Icon/Icon";
import { observer } from "mobx-react";
import React from "react";
import { T } from "utils/translation";
import { ModalWindow } from "../Dialog/Dialog";
import S from "gui/Components/Dialogs/SearchDialog.module.scss";
import { observable } from "mobx";
import { getApi } from "model/selectors/getApi";
import { ISearchResult } from "model/entities/types/ISearchResult";

@observer
export class SearchDialog extends React.Component<{
  ctx: any;
  onCloseClick: () => void;
  onSearchResultsChange: (results: ISearchResult[]) => void;
}> {

  input: HTMLInputElement | undefined;
  refInput = (elm: HTMLInputElement) => (this.input = elm);

  componentDidMount(){
    this.input?.focus();
  }

  onKeyDown(event: any){
    if(event.key === "Escape"){
      this.props.onCloseClick();
    }
  }

  @observable
  value = "";

  async onInputKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key == "Enter" && this.value.trim()) {
      const api = getApi(this.props.ctx);
      const searchResults = await api.search(this.value);
      this.props.onSearchResultsChange(searchResults);
    }
  }

  onChange(event: React.ChangeEvent<HTMLInputElement>): void {
    this.value = event.target.value;
  }

  render() {
    return (
      <ModalWindow
        title={null}
        titleButtons={null}
        buttonsCenter={null}
        onKeyDown={(event:any) => this.onKeyDown(event)}
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={S.root}>
          <div className={S.inputRow}>
            <Icon className={S.icon} src="./icons/search.svg" />
            <input
              ref={this.refInput}
              className={S.input}
              placeholder="Search for anything here"
              onKeyDown={(event) => this.onInputKeyDown(event)}
              onChange={(event) => this.onChange(event)}
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
          {this.isExpanded && this.props.items.map(item => <ResultItem name={item} key={item}/> )}
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

