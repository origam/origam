export class FocusManager {
  objectMap: Map<string, IFocusable> = new Map<string, IFocusable>();
  focusableContainers: IFocusableObjectContainer[] = [];

  subscribe(focusableObject: IFocusable, name: string | undefined, tabIndex: string | undefined) {
    const focusableContainer = new FocusableObjectContainer(focusableObject, name, tabIndex);
    this.focusableContainers.push(focusableContainer);
    this.focusableContainers = this.focusableContainers.sort(FocusableObjectContainer.compare);
    return () => this.unsubscribe(focusableContainer);
  }

  private unsubscribe(container: IFocusableObjectContainer) {
    const index = this.focusableContainers.indexOf(container);
    if (index > -1) {
      this.focusableContainers.splice(index, 1);
    }
  }

  focus(name: string) {
    this.focusableContainers.find((container) => container.name === name)?.focusable.focus();
  }

  focusFirst() {
    if (this.focusableContainers.length === 0) {
      return;
    }
    const focusable = this.focusableContainers[0].focusable;
    setTimeout(() => {
      focusable.focus();
    }, 0);
  }

  focusNext(activeElement: any) {
    const currentContainerIndex = this.focusableContainers.findIndex(
      (container) => container.focusable === activeElement
    );
    const nextIndex =
      this.focusableContainers.length - 1 > currentContainerIndex ? currentContainerIndex + 1 : 0;
    const focusable = this.focusableContainers[nextIndex].focusable;
    if(focusable.disabled){
      this.focusNext(focusable);
    }
    else{
      focusable.focus();
    }
  }

  focusPrevious(activeElement: any) {
    const currentContainerIndex = this.focusableContainers.findIndex(
      (container) => container.focusable === activeElement
    );
    const previosIndex =
      currentContainerIndex === 0 ? this.focusableContainers.length - 1 : currentContainerIndex - 1;
    console.log("nextIndex: "+previosIndex)
    const focusable = this.focusableContainers[previosIndex].focusable;
    if(focusable.disabled){
      this.focusPrevious(focusable);
    }
    else{
      focusable.focus();
    }
  }
}

interface IFocusableObjectContainer {
  name: string | undefined;
  tabIndexFractions: number[];
  focusable: IFocusable;
}

class FocusableObjectContainer implements IFocusableObjectContainer {
  get tabIndexFractions(): number[] {
    if (this.tabIndexNullable) {
      return this.tabIndexNullable
        .split(".")
        .filter((x) => x != "")
        .map((x) => parseInt(x));
    }
    return [1e6];
  }

  constructor(
    public focusable: IFocusable,
    public name: string | undefined,
    private tabIndexNullable: string | undefined
  ) {}

  // TabIndex is a string separated by decimal points for example: 13, 14.0, 14.2, 14.15
  // The "fractions" have to be compared separately because 14.15 is greater than 14.2
  // Comparison as numbers would give different results
  static compare(x: IFocusableObjectContainer, y: IFocusableObjectContainer) {
    const firstFraction = x.tabIndexFractions[0] - y.tabIndexFractions[0];
    if (firstFraction !== 0) {
      return firstFraction;
    }
    if (x.tabIndexFractions.length > y.tabIndexFractions.length) {
      return 1;
    }
    if (x.tabIndexFractions.length < y.tabIndexFractions.length) {
      return -1;
    }
    return x.tabIndexFractions[1] - y.tabIndexFractions[1];
  }
}

export interface IFocusable {
  focus(): void;
  disabled: boolean;
  tabIndex: number;
}
