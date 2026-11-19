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
import {
  compareTabIndexOwners,
  ITabIndexOwner,
  TabIndex,
} from "model/entities/TabIndexOwner";
import { requestFocus } from "utils/focus";
import { getDataView } from "model/selectors/DataView/getDataView";

export class FormFocusManager {
  autoFocusDisabled = false;
  backupFocusPlaceholder: HTMLInputElement | undefined | null;

  private isFormPerspectiveActive() {
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

  constructor(public parent: any) {}

  subscribe(
    focusableObject: IFocusable,
    name: string | undefined,
    tabIndex: TabIndex,
    onBlur?: () => Promise<void>,
  ) {
    if (!focusableObject) {
      return;
    }
    const focusableContainer = new FocusableObjectContainer(
      focusableObject,
      name,
      tabIndex,
      onBlur,
    );
    const existingContainer = this.focusableContainers.find(
      (container) =>
        (container.name && container.name === name) ||
        container.focusable === focusableObject,
    );
    if (existingContainer) {
      this.focusableContainers.remove(existingContainer);
    }
    this.focusableContainers.push(focusableContainer);
    this.focusableContainers = this.focusableContainers.sort(
      compareTabIndexOwners,
    );
  }

  setBackupFocusPlaceHolder(input: HTMLInputElement | null) {
    this.backupFocusPlaceholder = input;
  }

  focus(name: string) {
    let focusable = this.focusableContainers.find(
      (container) => container.name === name,
    )?.focusable;
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
    if (this.focusableContainers.length === 0) {
      setTimeout(() => {
        requestFocus(this.backupFocusPlaceholder);
      }, 0);
      return;
    }
    this.focusInDirection(undefined, 1);
  }

  autoFocus() {
    const nothingToFocus =
      this.focusableContainers.length === 0 && !this.backupFocusPlaceholder;
    if (
      nothingToFocus ||
      this.autoFocusDisabled ||
      isGlobalAutoFocusDisabled(this.parent)
    ) {
      return;
    }
    this.forceAutoFocus();
  }

  focusNext(activeElement: any) {
    this.focusInDirection(activeElement, 1);
  }

  focusPrevious(activeElement: any) {
    this.focusInDirection(activeElement, -1);
  }

  private focusInDirection(activeElement: any, direction: 1 | -1) {
    const currentContainerIndex = this.focusableContainers.findIndex(
      (container) => container.focusable === activeElement,
    );
    const focusable = this.findFocusableElement(
      currentContainerIndex,
      direction,
    );
    if (!focusable) {
      return;
    }
    setTimeout(() => {
      this.focusAndRemember(focusable);
    });
  }

  private findFocusableElement(
    currentContainerIndex: number,
    direction: 1 | -1,
  ): IFocusable | undefined {
    const count = this.focusableContainers.length;
    let index = currentContainerIndex;

    if (currentContainerIndex < 0 && direction === -1) {
      // Start before the last field when navigating backward
      // from an element that isn't registered.
      index = 0;
    }

    // Visit each field at most once, including when none can receive focus.
    for (let checked = 0; checked < count; checked++) {
      // Move in the requested direction and wrap around at either end.
      // Adding count keeps the remainder nonnegative when moving backward.
      index = (index + direction + count) % count;
      const focusable = this.focusableContainers[index].focusable;
      if (!focusable.disabled && focusable.checkVisibility()) {
        return focusable;
      }
    }
    return undefined;
  }

  async activeEditorCloses() {
    const lastFocusedContainer = this.focusableContainers.find(
      (x) => x.focusable === this.lastFocused,
    );
    if (lastFocusedContainer?.onBlur) {
      await lastFocusedContainer.onBlur();
    }
  }
}

class FocusableObjectContainer implements ITabIndexOwner {
  constructor(
    public focusable: IFocusable,
    public name: string | undefined,
    public tabIndex: TabIndex,
    public onBlur?: () => Promise<void>,
  ) {}
}

class EditorContainer {
  constructor(
    public focusable: IFocusable,
    public onBlur: () => Promise<void>,
  ) {}
}

export interface IFocusable {
  focus(): void;
  checkVisibility: () => boolean;
  disabled: boolean;
}
