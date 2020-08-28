export class FocusManager {
  objectMap: Map<string, IFocusable> = new Map<string, IFocusable>();

  subscribe(focusableObject: IFocusable, name: string | undefined) {
    if(!name){
      return () => {};
    }
    this.objectMap.set(name, focusableObject);
    return () => this.unsubscribe(name);
  }

  unsubscribe(name: string | undefined) {
    if(!name){
      return;
    }
    this.objectMap.delete(name);
  }

  focus(name: string) {
    const objectToFocus = this.objectMap.get(name);
    if (objectToFocus) {
      objectToFocus.focus();
    }
  }

  focusFirst(){
    if(this.objectMap.size === 0){
      return;
    }
    setTimeout(() => {
      const iFocusable = Array.from(this.objectMap.values())
        .sort((x, y) => x.tabIndex - y.tabIndex)
        [0];
      iFocusable.focus();
    }, 0)
  }
}

export interface IFocusable {
  focus(): void;
  tabIndex: number;
}
