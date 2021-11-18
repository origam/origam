export interface IParented<TParent> {
  parent?: TParent;
}

export function proxyEnrich<TAdd, TTarget extends object & IParented<TAdd>>(
  target: TTarget
) {
  return new Proxy<TTarget>(target, {
    get(obj, prop) {
      return prop in obj ? (obj as any)[prop] : (obj.parent as any)[prop];
    },
    set(obj, prop, newVal) {
      if (prop in obj) {
        (obj as any)[prop] = newVal;
      } else {
        (obj.parent as any)[prop] = newVal;
      }
      return true;
    }
  }) as TTarget & TAdd;
}