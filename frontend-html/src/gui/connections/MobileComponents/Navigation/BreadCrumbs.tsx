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

import React, { useContext } from "react";
import S from "gui/connections/MobileComponents/Navigation/BreadCrumbs.module.scss";
import { MobXProviderContext, observer } from "mobx-react";
import { BreadCrumbsState } from "model/entities/MobileState/BreadCrumbsState";

export const BreadCrumbs: React.FC<{}> = observer((props) => {
  const breadCrumbsState = useContext(MobXProviderContext).application.mobileState.breadCrumbsState as BreadCrumbsState;
  const breadCrumbList = breadCrumbsState.visibleNodes;

  function makeBreadcrumb(node: IBreadCrumbNode, index: number) {
    let className = index === breadCrumbList.length - 1 ? "" : S.breadcrumb;
    if(node.disabled) className += " " + S.disabled
    return (
      <div
        className={className}
        key={node.id}
        onClick={() =>{
          if(!node.disabled){
            node.onClick()
          }
        }}
      >
        {node.caption}
      </div>);
  }

  return (
    <div className={S.navigationBar}>
      {breadCrumbList.length > 0 &&
        breadCrumbList
          .flatMap((node, i) => i === 0
            ? [makeBreadcrumb(node, i)]
            : [<div key={node.id + "Sep"} className={S.pathSeparator}>/</div>,
              makeBreadcrumb(node, i)
            ]
          )
      }
    </div>
  );
});

export interface IBreadCrumbNode {
  disabled: boolean;
  caption: string;
  id: string;
  isVisible: () => boolean;
  onClick: () => void;
}

export class BreadCrumbNode implements IBreadCrumbNode {
  isVisible = () => true;

  constructor(
    public caption: string,
    public id: string,
    public onClick: () => void,
    public disabled: boolean
  ) {
  }
}

export class RootBreadCrumbNode implements IBreadCrumbNode {
  constructor(
    public getCaption: () => string,
  ) {
  }

  id = "root";
  disabled = false;

  onClick = () => {
  };

  isVisible = () => true;

  get caption(){
    return this.getCaption();
  }
}