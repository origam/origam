export class CheckboxDataCellRenderer {
  render() {
    ctx.font = `${14 * CPR}px "Font Awesome 5 Free"`;
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(
      !!cell.value ? "\uf14a" : "\uf0c8",
      (columnWidth / 2) * CPR,
      (rowHeight / 2) * CPR
    );
  }
}