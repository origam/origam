import { createContext, PropsWithChildren, useContext, useMemo, useState } from "react";
import _ from "lodash";
import { Observer } from "mobx-react";
import React from "react";

export class ResponsiveBlock {
  constructor(private onChildrenSetUpdate?: (ids: Set<any>) => void) {
    this.refContainer = this.refContainer.bind(this);
    this.refChild = this.refChild.bind(this);
  }

  hiddenChildren = new Set<any>();

  childToKey = new Map<any, any>();
  keyToChildRec = new Map<
    any,
    {
      elmChild: any;
      width: number;
      order: number | undefined;
      compensate: number;
      setHidden: (state: boolean) => void;
    }
  >();

  container: any;
  containerWidth = Number.MAX_SAFE_INTEGER;
  containerCompensate = 0;

  recomputeSizesImm() {
    const keysAndChildren = Array.from(this.keyToChildRec);
    keysAndChildren.sort(([ak, ar], [bk, br]) => {
      if (ar.order === br.order) return 0;
      if (ar.order === undefined) return 1;
      if (br.order === undefined) return -1;
      return ar.order - br.order;
    });

    const hiddenChildrenPruned = new Set(this.hiddenChildren);
    this.hiddenChildren = new Set();
    let widthAcc = this.containerCompensate;
    for (let [k, v] of keysAndChildren) {
      widthAcc = widthAcc + v.width + v.compensate;
      if (widthAcc > this.containerWidth) {
        this.hiddenChildren.add(k);
        hiddenChildrenPruned.delete(k);
        v.setHidden(true);
      }
    }
    for (let k of hiddenChildrenPruned.keys()) {
      this.keyToChildRec.get(k)?.setHidden(false);
    }
    this.onChildrenSetUpdate?.(this.hiddenChildren);
  }

  recomputeSizesDeb = _.throttle(this.recomputeSizesImm.bind(this), 500);

  someNodesResizedImm(entries: any[]) {
    let shouldRecompute = false;
    if (this.container) {
      const newWidth = this.container.getBoundingClientRect().width;
      if (newWidth !== this.containerWidth) shouldRecompute = true;
      this.containerWidth = newWidth;
    }
    for (let [chRec] of this.keyToChildRec.entries()) {
      if (chRec.elmChild) {
        const newWidth = chRec.elmChild.offsetWidth; /*e.target.getBoundingClientRect().width;*/
        if (chRec.width !== newWidth) {
          shouldRecompute = true;
          //console.log(e.target, e.target.offsetWidth, e.target.getBoundingClientRect().width, e);
        }
        chRec.width = newWidth;
      }
    }
    // for (let e of entries) {
    //   //console.log(e.target, e.contentRect.width);
    //   if (e.target === this.container) {
    //     const newWidth = e.contentRect.width;
    //     if (newWidth !== this.containerWidth) shouldRecompute = true;
    //     this.containerWidth = newWidth;
    //     continue;
    //   }
    //   const key = this.childToKey.get(e.target);
    //   // Preserve width when hidden, otherwise it gets never shown again.
    //   if (this.hiddenChildren.has(key)) continue;
    //   const childRec = this.keyToChildRec.get(key);
    //   if (childRec) {
    //     const newWidth = e.target.offsetWidth;// e.target.getBoundingClientRect().width;
    //     if (childRec.width !== newWidth) {
    //       shouldRecompute = true;
    //       console.log(e.target, e.target.offsetWidth, e.target.getBoundingClientRect().width, e);
    //     }
    //     childRec.width = newWidth;
    //     continue;
    //   } else {
    //     console.log("no child rec");
    //   }
    // }

    // console.log(
    //   "chw",
    //   this.containerWidth,
    //   Array.from(this.keyToChildRec).map(([k, v]) => `${k}:${v.width}`)
    //   /*Array.from(this.keyToChildRec)
    //     .map(([k, v]) => v.width)
    //     .reduce((acc, v) => acc + v, 0)*/
    // );
    if (shouldRecompute) this.recomputeSizesDeb();
  }

  someNodesResizedDeb = _.debounce(this.someNodesResizedImm.bind(this), 100);

  domObsv = new (window as any).ResizeObserver(this.someNodesResizedDeb.bind(this));

  refContainer(compensate: number, elm: any) {
    if (elm) {
      this.container = elm;
      this.containerCompensate = compensate;
      this.domObsv.observe(elm);
    } else {
      this.domObsv.disconnect();
      this.containerCompensate = 0;
      this.container = elm;
    }
  }

  refChild(
    key: any,
    order: any,
    compensate: number,
    setHidden: (state: boolean) => void,
    elm: any
  ) {
    if (elm) {
      this.childToKey.set(elm, key);
      this.keyToChildRec.set(key, { elmChild: elm, width: 0, order, compensate, setHidden });
      this.domObsv.observe(elm);
    } else {
      const childRec = this.keyToChildRec.get(key);
      if (childRec) this.domObsv.unobserve(childRec.elmChild);
      this.childToKey.delete(elm);
      this.keyToChildRec.delete(key);
    }
  }

  isHiddenChild(key: any) {
    return this.hiddenChildren.has(key);
  }
}

export function ResponsiveChild(
  props: PropsWithChildren<{
    childKey: any;
    order?: any;
    compensate?: number;
    children: (args: { refChild: any; isHidden: boolean }) => any;
  }>
) {
  const [isHidden, setHidden] = useState(false);
  const responsiveToolbar = useContext(CtxResponsiveToolbar);
  const refChild = useMemo(
    () => (elm: any) => {
      responsiveToolbar.refChild(
        props.childKey,
        props.order,
        props.compensate || 0,
        setHidden,
        elm
      );
    },
    [props.childKey]
  );
  return <Observer>{() => <>{props.children({ refChild, isHidden })}</>}</Observer>;
}

export function ResponsiveContainer(
  props: PropsWithChildren<{ compensate?: number; children: (args: { refChild: any }) => any }>
) {
  const responsiveToolbar = useContext(CtxResponsiveToolbar);
  return (
    <Observer>
      {() => (
        <>
          {props.children({
            refChild: (elm: any) => responsiveToolbar.refContainer(props.compensate || 0, elm),
          })}
        </>
      )}
    </Observer>
  );
}

export const CtxResponsiveToolbar = createContext<ResponsiveBlock>(new ResponsiveBlock());
