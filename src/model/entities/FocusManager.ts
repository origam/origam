export class FocusManager {
  objectMap: Map<string, IFocusable> = new Map<string, IFocusable>();
  focusableContainers: IFocusableObjectContainer[] = [];

  subscribe(focusableObject: IFocusable, name: string | undefined) {
    const focusableContainer = { name: name, focusable: focusableObject };
    this.focusableContainers.push(focusableContainer);
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
    const nextIndex = this.focusableContainers.length - 1 > currentContainerIndex
      ? currentContainerIndex + 1
      : 0;
    this.focusableContainers[nextIndex].focusable.focus();
  }

  focusPrevious(activeElement: any) {
    const currentContainerIndex = this.focusableContainers.findIndex(
      (container) => container.focusable === activeElement
    );
    const nextIndex = currentContainerIndex === 0
      ? this.focusableContainers.length - 1
      : currentContainerIndex - 1;
    this.focusableContainers[nextIndex].focusable.focus();
  }
}

interface IFocusableObjectContainer {
  name: string | undefined;
  focusable: IFocusable;
}

export interface IFocusable {
  focus(): void;

  tabIndex: number;
}
