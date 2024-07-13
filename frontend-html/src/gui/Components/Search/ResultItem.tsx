/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.
This file is part of ORIGAM (http://www.origam.org).
ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

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