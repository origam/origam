
const canvas = document.createElement("canvas");
const context = canvas.getContext("2d") as any;
export function getCanvasPixelRatio() {
  const devicePixelRatio = window.devicePixelRatio || 1;
  const backingStoreRatio =
    context!.webkitBackingStorePixelRatio ||
    context!.mozBackingStorePixelRatio ||
    context!.msBackingStorePixelRatio ||
    context!.oBackingStorePixelRatio ||
    context!.backingStorePixelRatio ||
    1;

  const ratio = devicePixelRatio / backingStoreRatio;
  return ratio;
}


export const CPR = () =>  getCanvasPixelRatio();
