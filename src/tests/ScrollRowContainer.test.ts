import {ScrollRowContainer} from "../model/entities/ScrollRowContainer";


function idGetter(row: any[]){
  return row[0];
}


function getTestValue(row: any[]){
  return row[1];
}

test('Should append 6 rows', () => {
  const scrollContainer = new ScrollRowContainer(idGetter);
  scrollContainer.appendRecords([
    ["1", "original"],
    ["2", "original"],
    ["3", "original"]]);
  scrollContainer.appendRecords([
    ["4", "original"],
    ["5", "original"],
    ["6", "original"]]);
  expect(scrollContainer.rows.length).toBe(6);
});

test('Should replace duplicate rows', () => {
  const scrollContainer = new ScrollRowContainer(idGetter);
  scrollContainer.appendRecords([
    ["1", "original"],
    ["2", "original"],
    ["3", "original"]]);
  scrollContainer.appendRecords([
    ["4", "original"],
    ["5", "original"],
    ["6", "original"]]);

  scrollContainer.appendRecords([
    ["2", "duplicate"],
    ["7", "new"],
    ["6", "duplicate"]]);

  expect(scrollContainer.rows.length).toBe(7);
  expect(scrollContainer.rowChunks.length).toBe(3);
  expect(getTestValue(scrollContainer.rowChunks[0].getRow("1"))).toBe("original");
  expect(getTestValue(scrollContainer.rowChunks[0].getRow("2"))).toBe("duplicate");
  expect(getTestValue(scrollContainer.rowChunks[0].getRow("3"))).toBe("original");
  expect(getTestValue(scrollContainer.rowChunks[1].getRow("4"))).toBe("original");
  expect(getTestValue(scrollContainer.rowChunks[1].getRow("5"))).toBe("original");
  expect(getTestValue(scrollContainer.rowChunks[1].getRow("6"))).toBe("duplicate");
  expect(getTestValue(scrollContainer.rowChunks[2].getRow("7"))).toBe("new");
});
