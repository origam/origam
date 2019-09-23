export function flashColor2htmlColor(flashColor: number) {
  return (
    "#" +
    (flashColor < 0 ? flashColor + 0xffffffff + 1 : flashColor)
      .toString(16)
      .toUpperCase()
      .padStart(6, "0")
  );
}
