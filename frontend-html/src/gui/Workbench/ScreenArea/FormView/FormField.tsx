import S from "./FormField.module.css";
import React from "react";
import {inject, observer} from "mobx-react";
import {getSelectedRowId} from "model/selectors/TablePanelView/getSelectedRowId";
import {getRowStateAllowRead} from "model/selectors/RowState/getRowStateAllowRead";

export enum ICaptionPosition {
  Left = "Left",
  Right = "Right",
  Top = "Top",
  None = "None"
}

@inject(({ property }) => {
  const rowId = getSelectedRowId(property);
  const isHidden = !getRowStateAllowRead(property, rowId || "", property.id);
  return {
    isHidden
  };
})
@observer
export class FormField extends React.Component<{
  Id: string;
  Name: string;
  CaptionLength: number;
  CaptionPosition: ICaptionPosition | undefined;
  Column: string;
  Entity: string;
  Height: number;
  Width: number;
  Y: number;
  X: number;
  isHidden?: boolean;
}> {
  fieldNameStyle() {
    // TODO: !!! Proper typing of props (numbers instead of strings etc...)

    switch (this.props.CaptionPosition) {
      case ICaptionPosition.Left:
        return {
          top: this.props.Y,
          left: this.props.X - this.props.CaptionLength,
          width: this.props.CaptionLength
          //  height: this.props.height
        };
      case ICaptionPosition.Right:
        return {
          top: this.props.Y,
          left:
            this.props.Column === "CheckBox"
              ? this.props.X + this.props.Height
              : this.props.X + this.props.Width,
          width: this.props.CaptionLength
          //  height: this.props.height
        };
      case ICaptionPosition.Top:
        return {
          top: this.props.Y - 20,
          left: this.props.X,
          width: this.props.CaptionLength
        };
      default:
        return {};
    }
  }

  render() {
    const isHidden = this.props.isHidden;
    const display = isHidden ? "none" : undefined;
    return (
      <>
        {this.props.CaptionPosition !== ICaptionPosition.None && (
          <div
            className={S.formFieldName}
            style={{ ...this.fieldNameStyle(), display }}
          >
            {this.props.Name}
          </div>
        )}
        <div
          className={S.formField}
          style={{
            top: this.props.Y,
            left: this.props.X,
            width: this.props.Width,
            height: this.props.Height,
            display
          }}
          onClick={(event: any) => event.stopPropagation()}
        >
          {this.props.children ? (
            this.props.children
          ) : (
            <div className={S.unknownEditor}>{this.props.Id}</div>
          )}
        </div>
      </>
    );
  }
}
