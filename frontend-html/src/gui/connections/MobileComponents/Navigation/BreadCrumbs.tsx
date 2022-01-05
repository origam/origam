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
import S from "./BreadCrumbs.module.scss";
import { MobXProviderContext, observer } from "mobx-react";
import { MobileState } from "model/entities/MobileState";

export const BreadCrumbs: React.FC<{}> = observer((props) => {
  const mobileState = useContext(MobXProviderContext).application.mobileState as MobileState;

  function makeBreadcrumb(node: IBreadCrumbNode, index: number) {
    return <a
      className={index === mobileState.breadCrumbList.length-1 ? "" : S.breadcrumb}
      key={node.caption}
      onClick={node.onClick}
    >
      {node.caption}
    </a>
  }

  return(
    <div className={S.navigationBar}>
      {mobileState.breadCrumbList.length > 0 &&
        mobileState.breadCrumbList
          .flatMap((node, i) => i === 0
            ? [makeBreadcrumb(node, i)]
            : [<div key={node.caption+"Sep"} className={S.pathSeparator}>/</div>,
              makeBreadcrumb(node, i)
            ]
          )
      }
    </div>
  );
});

export interface IBreadCrumbNode{
  caption: string,
  onClick: ()=>void
}

export class BreadCrumbNode {
  constructor(
    public caption: string,
    public onClick: ()=>void
  ) {
  }
}

export class PassiveBreadCrumbNode implements IBreadCrumbNode{
  onClick = ()=>{};

  constructor(
    public caption: string,
  ) {
  }
}
