export class DateTimeDataCellRenderer {
  render() {
    if (cell.value !== null) {
      ctx.fillText(
        moment(cell.value).format(cell.formatterPattern),
        cellPaddingLeft * CPR,
        15 * CPR
      );
    }
  }
}