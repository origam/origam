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

import { isGlobalAutoFocusDisabled } from "model/actions-ui/ScreenToolbar/openSearchWindow";
import { compareTabIndexOwners, ITabIndexOwner, TabIndex } from "model/entities/TabIndexOwner";
import { requestFocus } from "utils/focus";
import { getDataView } from "model/selectors/DataView/getDataView";

export class FormFocusManager {
  autoFocusDisabled = false;
  backupFocusPlaceholder: HTMLInputElement | undefined | null;

  private isFormPerspectiveActive(){
    const dataView = getDataView(this.parent);
    return dataView.isFormViewActive();
  }

  stopAutoFocus() {
    this.autoFocusDisabled = true;
  }

  focusableContainers: FocusableObjectContainer[] = [];
  public lastFocused: IFocusable | undefined;

  setLastFocused(focusable: IFocusable) {
    this.lastFocused = focusable;
  }

  constructor(public parent: any) {
  }

  subscribe(focusableObject: IFocusable, name: string | undefined,
            tabIndex: TabIndex,
            onBlur?: ()=>Promise<void>) {
    if (!focusableObject) {
      return;
    }
    const focusableContainer = new FocusableObjectContainer(focusableObject, name,  tabIndex, onBlur);
    const existingContainer = this.focusableContainers
      .find(container => container.name && container.name === name ||
        container.focusable === focusableObject);
    if (existingContainer) {
      this.focusableContainers.remove(existingContainer);
    }
    this.focusableContainers.push(focusableContainer);
    this.focusableContainers = this.focusableContainers.sort(compareTabIndexOwners);
  }

  setBackupFocusPlaceHolder(input: HTMLInputElement | null) {
    this.backupFocusPlaceholder = input
  }

  focus(name: string) {
    let focusable = this.focusableContainers.find((container) => container.name === name)?.focusable;
    this.focusAndRemember(focusable);
  }

  private focusAndRemember(focusable: IFocusable | undefined) {
    if (!focusable) {
      return;
    }
    this.lastFocused = focusable;
    if (this.isFormPerspectiveActive()) {
      requestFocus(focusable as any);
    }
  }

  refocusLast() {
    if (this.isFormPerspectiveActive()) {
      requestFocus(this.lastFocused as any);
    }
  }

  forceAutoFocus() {
    if (this.focusableContainers.length === 0 && this.backupFocusPlaceholder) {
      setTimeout(() => {
        requestFocus(this.backupFocusPlaceholder);
      }, 0);
      return;
    }
    const focusable = this.focusableContainers[0].focusable;
    if (focusable.disabled) {
      //  (focusable as any).readOnly returns always false => readonly fields cannot be skipped
      this.focusNext(focusable);
      return;
    }
    setTimeout(() => {
      this.focusAndRemember(focusable);
    }, 0);
  }

  autoFocus() {
    const nothingToFocus = this.focusableContainers.length === 0 && !this.backupFocusPlaceholder;
    if (nothingToFocus || this.autoFocusDisabled || isGlobalAutoFocusDisabled(this.parent)) {
      return;
    }
    this.forceAutoFocus();
  }

  focusNext(activeElement: any) {
    this.focusNextInternal(activeElement, 0);
  }

  focusNextInternal(activeElement: any, callNumber: number) {
    if (callNumber > 20) {
      return;
    }
    const currentContainerIndex = this.focusableContainers.findIndex(
      (container) => container.focusable === activeElement
    );
    const focusable = this.findNextVisibleElement(currentContainerIndex);
    if (focusable !== activeElement && focusable.disabled) {
      this.focusNextInternal(focusable, callNumber + 1);
    } else {
      setTimeout(() => {
        this.focusAndRemember(focusable);
      });
    }
  }

  findNextVisibleElement(currentContainerIndex: number): IFocusable{
    const nextIndex =
      this.focusableContainers.length - 1 > currentContainerIndex ? currentContainerIndex + 1 : 0;
    const focusable = this.focusableContainers[nextIndex].focusable;
    if (!focusable.checkVisibility()) {
      return this.findNextVisibleElement(nextIndex);
    }
    return focusable;
  }

  focusPrevious(activeElement: any) {
    const currentContainerIndex = this.focusableContainers.findIndex(
      (container) => container.focusable === activeElement
    );
    const focusable = this.findPreviousVisibleElement(currentContainerIndex);
    if (focusable.disabled) {
      this.focusPrevious(focusable);
    } else {
      setTimeout(() => {
        this.focusAndRemember(focusable);
      });
    }
  }

  findPreviousVisibleElement(currentContainerIndex: number): IFocusable{
    const previousIndex =
      currentContainerIndex === 0 ? this.focusableContainers.length - 1 : currentContainerIndex - 1;
    const focusable = this.focusableContainers[previousIndex].focusable;
    if (!focusable.checkVisibility()) {
      return this.findPreviousVisibleElement(previousIndex);
    }
    return focusable;
  }

  async activeEditorCloses(){
    const lastFocusedContainer = this.focusableContainers.find(x => x.focusable === this.lastFocused);
    if(lastFocusedContainer?.onBlur){
      await lastFocusedContainer.onBlur();
    }
  }
}

class FocusableObjectContainer implements ITabIndexOwner {
  constructor(
    public focusable: IFocusable,
    public name: string | undefined,
    public tabIndex: TabIndex,
    public onBlur?: ()=>Promise<void>
  ) {
  }
}

class EditorContainer {
  constructor(
    public focusable: IFocusable,
    public onBlur: () => Promise<void>
  ) {
  }
}

export interface IFocusable {
  focus(): void;
  checkVisibility: ()=> boolean,
  disabled: boolean;
}
