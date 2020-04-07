export class ComboBoxDataCellRenderer {
  render() {
    if (cell.isLink) {
      ctx.save();
      ctx.fillStyle = "blue";
    }
    if (cell.value !== null) {
      ctx.fillText("" + cell.text!, cellPaddingLeft * CPR, 15 * CPR);
    }
    if (cell.isLink) {
      ctx.restore();
    }
  }
}