import React, { useContext } from "react";
import { Tooltip } from "react-tippy";

import CS from "./CommonStyle.module.css";
import S from "./TagInputEditor.module.css";

import {
  TagInput,
  TagInputAdd,
  TagInputItem,
  TagInputItemDelete,
} from "gui/Components/TagInput/TagInput";
import { inject, observer } from "mobx-react";
import { IProperty } from "model/entities/types/IProperty";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { CtxDropdownEditor } from "../../../../modules/Editors/DropdownEditor/DropdownEditor";
import { CtxDropdownRefCtrl } from "../../../../modules/Editors/DropdownEditor/Dropdown/DropdownCommon";

@inject(({ property }: { property: IProperty }, { value }) => {
  const dataTable = getDataTable(property);
  return {
    textualValues: dataTable.resolveCellText(property, value),
  };
})
@observer
export class TagInputEditor extends React.Component<{
  value: string[];
  textualValues?: string[];
  isReadOnly: boolean;
  isInvalid: boolean;
  invalidMessage?: string;
  isFocused: boolean;
  backgroundColor?: string;
  foregroundColor?: string;
  refocuser?: (cb: () => void) => () => void;
  onChange?(event: any, value: any): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onEditorBlur?(event: any): void;
}> {
  removeItem(event: any, item: string) {
    const index = this.props.value.indexOf(item);
    if (index > -1) {
      this.props.value.splice(index, 1);
      if (this.props.onChange) {
        this.props.onChange(event, this.props.value);
      }
      if (this.props.onEditorBlur) {
        this.props.onEditorBlur(event);
      }
    }
  }

  render() {
    return (
      <div className={CS.editorContainer}>
        <TagInput className={S.tagInput}>
          {this.props.value
            ? this.props.value.map((valueItem, idx) => (
                <TagInputItem key={valueItem}>
                  <TagInputItemDelete
                    onClick={(event) => {
                      this.removeItem(event, valueItem);
                    }}
                  />
                  {this.props.textualValues![idx] || ""}
                </TagInputItem>
              ))
            : null}
          <TagInputAdd />
        </TagInput>
        {/* <input
          style={{
            color: this.props.foregroundColor,
            backgroundColor: this.props.backgroundColor
          }}
          className={CS.editor}
          type="text"
          value={this.props.value || ""}
          readOnly={true || this.props.isReadOnly}
          // ref={this.refInput}
          onChange={(event: any) =>
            this.props.onChange &&
            this.props.onChange(event, event.target.value)
          }
          onKeyDown={this.props.onKeyDown}
          onClick={this.props.onClick}
          onBlur={this.props.onEditorBlur}
          // onFocus={this.handleFocus}
        />*/}
        {this.props.isInvalid && (
          <div className={CS.notification}>
            <Tooltip html={this.props.invalidMessage} arrow={true}>
              <i className="fas fa-exclamation-circle red" />
            </Tooltip>
          </div>
        )}
      </div>
    );
  }
}
