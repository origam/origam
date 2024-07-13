import { observer } from "mobx-react";
import React, { RefObject } from "react";
import { ISearchResult } from "model/entities/types/ISearchResult";
import { action, observable } from "mobx";
import S from "gui/Components/Search/ResultItem.module.scss";
import { Icon } from "gui/Components/Icon/Icon";

@observer
export class ResultItem extends React.Component<{
  result: ISearchResult;
  onResultItemClick: () => void;
  selected: boolean;
  registerElementRef: (id: string, ref: RefObject<HTMLDivElement>) => void;
}> {

  @observable
  mouseOver = false;

  divRef: RefObject<HTMLDivElement> = React.createRef();
  descriptionRef: RefObject<HTMLDivElement> = React.createRef();

  @observable
  isDescriptionTruncated = false;

  componentDidMount() {
    this.props.registerElementRef(this.props.result.id, this.divRef);
    this.checkDescriptionTruncation();
  }

  componentDidUpdate() {
    this.props.registerElementRef(this.props.result.id, this.divRef);
    this.checkDescriptionTruncation();
  }

  checkDescriptionTruncation = () => {
    if (this.descriptionRef.current) {
      const { offsetWidth, scrollWidth } = this.descriptionRef.current;
      debugger;
      this.isDescriptionTruncated = scrollWidth > offsetWidth;
    }
  }

  @action.bound
  onClick() {
    this.props.onResultItemClick();
    this.props.result.onClick();
  }

  render() {
    return (
      <div
        className={S.resultIemRow + " " + (this.mouseOver ? S.resultIemRowHovered : "") + " " +
          (this.props.selected ? S.resultIemRowSelected : "")}
        ref={this.divRef}
        onClick={() => this.onClick()}
        onMouseOver={evt => this.mouseOver = true}
        onMouseOut={evt => this.mouseOver = false}>
        <div className={S.itemIcon}>
          <Icon src={this.props.result.iconUrl}/>
        </div>
        <div className={S.itemContents}>
          <div className={S.itemTitle}>
            {this.props.result.label}
          </div>
          <div className={S.itemTextSeparator}/>
          <div
            className={S.itemDescription}
            ref={this.descriptionRef}
            title={this.isDescriptionTruncated ? this.props.result.description : undefined}
          >
            {this.props.result.description}
          </div>
        </div>
      </div>
    );
  }
}