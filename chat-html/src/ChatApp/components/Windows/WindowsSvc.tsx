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

import { action, computed, observable } from "mobx";
import { Observer } from "mobx-react";
import React from "react";
import { FullscreenCentered, Overlay } from "./Windows";

export interface IModalHandle<TInteractor> {
  close(): void;
  bringToFront(): void;
  interact(): Promise<TInteractor>;
  resolveInteract(interactor?: TInteractor): void;
}

export interface IWindowStackItem {
  key: any;
  render: (modal: IModalHandle<any>) => React.ReactNode;
  modalHandle: IModalHandle<any>;
}

let keyGen = 0;

export class WindowsSvc {
  @observable windowStack: IWindowStackItem[] = [];

  @computed get displaysWindow() {
    return this.windowStack.length > 0;
  }

  renderStack() {
    return (
      <Observer>
        {() => {
          const itemCnt = this.windowStack.length;
          const result: React.ReactNode[] = [];
          for (let i = 0; i < itemCnt; i++) {
            const window = this.windowStack[i];
            if (i === itemCnt - 1) {
              result.push(
                <Observer key={`Overlay_${window.key}`}>
                  {() => <Overlay />}
                </Observer>
              );
            }
            result.push(
              <Observer key={window.key}>
                {() => (
                  <FullscreenCentered key={window.key}>
                    {window.render(window.modalHandle)}
                  </FullscreenCentered>
                )}
              </Observer>
            );
          }
          return <>{result}</>;
        }}
      </Observer>
    );
  }

  @action.bound
  push<TInteractor>(
    render: (modal: IModalHandle<TInteractor>) => React.ReactNode
  ) {
    const myKey = keyGen++;
    let fnResolveInteract: any;
    const modalHandle = {
      close: action(() => {
        const idx = this.windowStack.findIndex((item) => item.key === myKey);
        if (idx > -1) this.windowStack.splice(idx, 1);
      }),
      bringToFront: action(() => {
        const idx = this.windowStack.findIndex((item) => item.key === myKey);
        if (idx > -1) {
          const item = this.windowStack.splice(idx, 1);
          this.windowStack.push(item[0]);
        }
      }),
      resolveInteract(interactor?: TInteractor) {
        fnResolveInteract?.(interactor);
      },
      interact() {
        return new Promise<TInteractor>((resolve) => {
          fnResolveInteract = resolve;
        });
      },
    };
    this.windowStack.push({
      key: myKey,
      modalHandle,
      render,
    });
    return modalHandle as IModalHandle<TInteractor>;
  }
}
