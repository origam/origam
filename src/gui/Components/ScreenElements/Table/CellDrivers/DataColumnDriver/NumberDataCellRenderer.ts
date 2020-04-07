export class NumberDataCellRenderer {
  render() {
    if (cell.value !== null) {
      ctx.save();
      ctx.textAlign = "right";
      ctx.fillText("" + cell.value!, (columnWidth - cellPaddingLeft) * CPR, 15 * CPR);
      ctx.restore();
    }
  }
}