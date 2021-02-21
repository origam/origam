import { observable } from "mobx";
import { observer } from "mobx-react";
import React from "react";
import S from "./ColorEditor.module.scss";

@observer
export default class ColorEditor extends React.Component<{}> {
  refContainer = (elm: any) => (this.elmContainer = elm);
  elmContainer: any;
  refInput = (elm: any) => (this.elmInput = elm);
  elmInput: any;

  @observable isDroppedDown = false;

  render() {
    return (
      <div
        className={S.editorContainer}
        ref={this.refContainer}
        style={{
          zIndex: this.isDroppedDown ? 1000 : undefined,
        }}
      >
        <input
          style={{}}
          className={S.input}
          type="text"
          //onBlur={this.handleInputBlur}
          //onFocus={this.handleFocus}

          ref={this.refInput}
          // value={this.textfieldValue}
          // readOnly={this.props.isReadOnly}
          // onChange={this.handleTextfieldChange}
          //onClick={this.props.onClick}
          //onDoubleClick={this.props.onDoubleClick}
          //onKeyDown={this.handleKeyDown}
        />
        <div className={S.dropdownSymbol}>
          {/*onMouseDown={() => setDropped(true)} ref={refTrigger}>*/}
          <i className="fas fa-palette" />
        </div>
      </div>
    );
  }
}
