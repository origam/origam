export function flashColor2htmlColor(flashColor: number) {
  if (flashColor === 0 || flashColor === null || flashColor === undefined) {
    return undefined;
  }
  return (
    "#" +
    (flashColor < 0 ? flashColor + 0xffffffff + 1 : flashColor)
      .toString(16)
      .slice(-6)
      .toUpperCase()
      .padStart(6, "0")
  );
}

export function htmlColor2FlashColor(htmlColor: string | undefined | null) {
  if (htmlColor === undefined || htmlColor === null) {
    return htmlColor;
  }
  if (htmlColor.startsWith("#")) {
    htmlColor = htmlColor.slice(1);
  }
  const flashColor = parseInt(htmlColor, 16);
  return flashColor;
}
