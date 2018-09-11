

export function getCanvasPixelRatio() {
  const canvas = document.createElement('canvas');
  const context = canvas.getContext('2d') as any;
  const devicePixelRatio = window.devicePixelRatio || 1;
  const backingStoreRatio = context!.webkitBackingStorePixelRatio ||
                      context!.mozBackingStorePixelRatio ||
                      context!.msBackingStorePixelRatio ||
                      context!.oBackingStorePixelRatio ||
                      context!.backingStorePixelRatio || 1;

  const ratio = devicePixelRatio / backingStoreRatio;
  return ratio
}

export const CPR = getCanvasPixelRatio();

export function trcpr(...args: number[]) {
  return args.map(x => x * CPR);
}