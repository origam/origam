import { observer } from "mobx-react";
import React from "react";
import { setAllSelectionStates } from "model/actions-tree/setAllSelectionStates";
import { CPR } from "utils/canvas";
import {
  checkSymbolFontSize,
  topTextOffset,
} from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";
import {IReactionDisposer, reaction} from "mobx";

@observer
export class SelectionCheckBoxHeader extends React.Component<{
  width: number;
  dataView: any;
}> {
  onClick(event: any) {
    this.props.dataView.selectAllCheckboxChecked = !this.props.dataView.selectAllCheckboxChecked;
    setAllSelectionStates(this.props.dataView, this.props.dataView.selectAllCheckboxChecked);

    this.drawSelectionCheckboxContent();
  }
  drawReactionDisposer: IReactionDisposer | undefined ;
  canvas: HTMLCanvasElement | null = null;
  refCanvas = (elm: HTMLCanvasElement | any) => {
    this.canvas = elm;
  };

  drawSelectionCheckboxContent() {
    const ctx2d = this.canvas!.getContext("2d")!;
    ctx2d.clearRect(0, 0, this.canvas!.width, this.canvas!.height);

    ctx2d.beginPath();
    ctx2d.fillStyle = "black";
    ctx2d.font = `${CPR() * checkSymbolFontSize}px "Font Awesome 5 Free"`;
    ctx2d.fillText(
      this.props.dataView.selectAllCheckboxChecked ? "\uf14a" : "\uf0c8",
      0,
      CPR() * topTextOffset
    );
    ctx2d.closePath();
  }

  componentDidMount() {
    this.drawSelectionCheckboxContent();

    this.drawReactionDisposer = reaction(
      () => this.props.dataView.selectAllCheckboxChecked,
      () => this.drawSelectionCheckboxContent()
    );
  }

  componentWillUnmount() {
    this.drawReactionDisposer && this.drawReactionDisposer();
  }

  render() {
    return (
      <div style={{ minWidth: this.props.width + "px" }}>
        <canvas
          ref={this.refCanvas}
          width={20}
          height={20}
          onClick={(event) => this.onClick(event)}
        />
      </div>
    );
  }
}

