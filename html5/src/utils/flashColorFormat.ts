export function flashColor2htmlColor(flashColor: number) {
  if (flashColor === 0) {
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
