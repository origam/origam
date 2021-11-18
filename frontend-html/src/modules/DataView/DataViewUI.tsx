import React from 'react';
import {TypeSymbol} from "dic/Container";
import {Observer} from "mobx-react";
import {IRenderable} from "modules/CommonTypes";
import {ContribArray} from "utils/common";
import {IDataViewToolbarContribItem} from "./DataViewTypes";

export class DataViewBodyUI {
  contrib = new ContribArray<IRenderable>();

  render() {
    return <Observer>{() => <>{this.contrib.asArray().map((item) => item.render())}</>}</Observer>;
  }
}

export const IDataViewBodyUI = TypeSymbol<DataViewBodyUI>("DataViewBodyUI");



export class DataViewToolbarUI {
  contrib = new ContribArray<IDataViewToolbarContribItem>();

  renderSection(section: string) {
    return (
      <Observer>
        {() => (
          <>
            {this.contrib
              .asArray()
              .filter((item) => item.section === section)
              .map((item) => item.render())}
          </>
        )}
      </Observer>
    );
  }
}

export const IDataViewToolbarUI = TypeSymbol<DataViewToolbarUI>("DataViewToolbarUI");
