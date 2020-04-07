export class DefaultValueTypeDataCellRenderer {
  render() {
    if (cell.value !== null) {
      if (!cell.isPassword) {
        ctx.fillText("" + cell.value!, cellPaddingLeft * CPR, 15 * CPR);
      } else {
        ctx.fillText("*******", cellPaddingLeft * CPR, 15 * CPR);
      }
    }
  }
}