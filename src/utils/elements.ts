export function getElementPosition(element: HTMLElement) {
  let el = element;
  if (!el) {
    throw new Error("Element is not defined.");
  }
  let xPosition = 0;
  let yPosition = 0;
  let firstCycle = true;
  while (el) {
    if (el.tagName === "BODY") {
      // deal with browser quirks with body/window/document and page scroll
      const xScrollPos = el.scrollLeft || document.documentElement.scrollLeft;
      const yScrollPos = el.scrollTop || document.documentElement.scrollTop;
      xPosition +=
        el.offsetLeft - (firstCycle ? 0 : xScrollPos) + el.clientLeft;
      yPosition += el.offsetTop - (firstCycle ? 0 : yScrollPos) + el.clientTop;
    } else {
      xPosition +=
        el.offsetLeft - (firstCycle ? 0 : el.scrollLeft) + el.clientLeft;
      yPosition +=
        el.offsetTop - (firstCycle ? 0 : el.scrollTop) + el.clientTop;
    }
    firstCycle = false;
    el = el.offsetParent as any;
  }
  return {
    x: xPosition,
    y: yPosition
  };
}