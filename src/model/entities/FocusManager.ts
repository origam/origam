import { isGlobalAutoFocusDisabled } from "model/actions-ui/ScreenToolbar/openSearchWindow";

export class FocusManager {
  autoFocusDisabled = false;

  stopAutoFocus() {
    this.autoFocusDisabled = true;
  }
  objectMap: Map<string, IFocusAble> = new Map<string, IFocusAble>();
  focusAbleContainers: IFocusAbleObjectContainer[] = [];

  constructor(public parent: any) {}

  subscribe(focusAbleObject: IFocusAble, name: string | undefined, tabIndex: string | undefined) {
    if(!focusAbleObject){
      return;
    }
    const focusAbleContainer = new FocusAbleObjectContainer(focusAbleObject, name, tabIndex);
    const existingContainer = this.focusAbleContainers
      .find(container => container.name && container.name === name ||
            container.focusAble === focusAbleObject);
    if(existingContainer){
      this.focusAbleContainers.remove(existingContainer);
    }
    this.focusAbleContainers.push(focusAbleContainer);
    this.focusAbleContainers = this.focusAbleContainers.sort(FocusAbleObjectContainer.compare);
  }

  focus(name: string) {
    this.focusAbleContainers.find((container) => container.name === name)?.focusAble.focus();
  }

  forceAutoFocus() {
    const focusAble = this.focusAbleContainers[0].focusAble;
    if (focusAble.disabled) {
      //  (focusAble as any).readOnly returns always false => readonly fields cannot be skipped
      this.focusNext(focusAble);
      return;
    }
    setTimeout(() => {
      focusAble.focus();
    }, 0);
  }

  autoFocus() {
    if (this.focusAbleContainers.length === 0 || this.autoFocusDisabled || isGlobalAutoFocusDisabled(this.parent)) {
      return;
    }
    this.forceAutoFocus();
  }

  focusNext(activeElement: any) {
      this.focusNextInternal(activeElement, 0);
  }

  focusNextInternal(activeElement: any, callNumber: number) {
    if(callNumber > 20){
      return;
    }
    const currentContainerIndex = this.focusAbleContainers.findIndex(
      (container) => container.focusAble === activeElement
    );
    const nextIndex =
      this.focusAbleContainers.length - 1 > currentContainerIndex ? currentContainerIndex + 1 : 0;
    const focusAble = this.focusAbleContainers[nextIndex].focusAble;
    if (focusAble !== activeElement && focusAble.disabled) {
      this.focusNextInternal(focusAble, callNumber + 1);
    } else {
      setTimeout(() => {
        focusAble.focus();
      });
    }
  }

  focusPrevious(activeElement: any) {
    const currentContainerIndex = this.focusAbleContainers.findIndex(
      (container) => container.focusAble === activeElement
    );
    const previousIndex =
      currentContainerIndex === 0 ? this.focusAbleContainers.length - 1 : currentContainerIndex - 1;
    const focusAble = this.focusAbleContainers[previousIndex].focusAble;
    if (focusAble.disabled) {
      this.focusPrevious(focusAble);
    } else {
      setTimeout(() => {
        focusAble.focus();
      });
    }
  }
}

export interface IFocusAbleObjectContainer {
  name: string | undefined;
  tabIndexFractions: number[];
  focusAble: IFocusAble;
  has(fractionIndex: number): boolean;
}

export class FocusAbleObjectContainer implements IFocusAbleObjectContainer {
  get tabIndexFractions(): number[] {
    if (this.tabIndexNullable) {
      return this.tabIndexNullable
        .split(".")
        .filter((x) => x !== "")
        .map((x) => parseInt(x));
    }
    return [1e6];
  }

  constructor(
    public focusAble: IFocusAble,
    public name: string | undefined,
    private tabIndexNullable: string | undefined
  ) {}

  // TabIndex is a string separated by decimal points for example: 13, 14.0, 14.2, 14.15
  // The "fractions" have to be compared separately because 14.15 is greater than 14.2
  // Comparison as numbers would give different results
  static compare(x: IFocusAbleObjectContainer, y: IFocusAbleObjectContainer) {
    return FocusAbleObjectContainer.compareFraction(x, y, 0);
  }

  static compareFraction(
    x: IFocusAbleObjectContainer,
    y: IFocusAbleObjectContainer,
    fractionIndex: number
  ): number {
    if (x.has(fractionIndex) && !y.has(fractionIndex)) {
      return 1;
    }
    if (!x.has(fractionIndex) && y.has(fractionIndex)) {
      return -1;
    }
    if (!x.has(fractionIndex) && !y.has(fractionIndex)) {
      return 0;
    }

    const fraction = x.tabIndexFractions[fractionIndex] - y.tabIndexFractions[fractionIndex];
    if (fraction !== 0) {
      return fraction;
    }

    return FocusAbleObjectContainer.compareFraction(x, y, fractionIndex + 1);
  }

  has(fractionIndex: number) {
    return this.tabIndexFractions.length - 1 >= fractionIndex;
  }
}

export interface IFocusAble {
  focus(): void;
  disabled: boolean;
}
