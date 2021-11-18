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

test('Should delete a row', () => {
  const scrollContainer = new ScrollRowContainer(idGetter);
  scrollContainer.appendRecords([
    ["1", "original"],
    ["2", "original"],
    ["3", "original"]]);
  scrollContainer.appendRecords([
    ["4", "original"],
    ["5", "original"],
    ["6", "original"]]);

  scrollContainer.delete(["5", "original"]);

  expect(scrollContainer.rows.length).toBe(5);
  expect(scrollContainer.rowChunks.length).toBe(2);
  expect(scrollContainer.rowChunks[0].getRow("1")[0]).toBe("1");
  expect(scrollContainer.rowChunks[0].getRow("2")[0]).toBe("2");
  expect(scrollContainer.rowChunks[0].getRow("3")[0]).toBe("3");
  expect(scrollContainer.rowChunks[1].getRow("4")[0]).toBe("4");
  expect(scrollContainer.rowChunks[1].getRow("5")).toBeUndefined();
  expect(scrollContainer.rowChunks[1].getRow("6")[0]).toBe("6");
});

test('Should delete last row', () => {
  const scrollContainer = new ScrollRowContainer(idGetter);
  scrollContainer.appendRecords([
    ["1", "original"],
    ["2", "original"],
    ["3", "original"]]);
  scrollContainer.appendRecords([
    ["4", "original"],
    ["5", "original"],
    ["6", "original"]]);

  scrollContainer.delete(["6", "original"]);

  expect(scrollContainer.rows.length).toBe(5);
  expect(scrollContainer.rowChunks.length).toBe(2);
  expect(scrollContainer.rowChunks[0].getRow("1")[0]).toBe("1");
  expect(scrollContainer.rowChunks[0].getRow("2")[0]).toBe("2");
  expect(scrollContainer.rowChunks[0].getRow("3")[0]).toBe("3");
  expect(scrollContainer.rowChunks[1].getRow("4")[0]).toBe("4");
  expect(scrollContainer.rowChunks[1].getRow("5")[0]).toBe("5");
  expect(scrollContainer.rowChunks[1].getRow("6")).toBeUndefined();
});

test('Should delete first row', () => {
  const scrollContainer = new ScrollRowContainer(idGetter);
  scrollContainer.appendRecords([
    ["1", "original"],
    ["2", "original"],
    ["3", "original"]]);
  scrollContainer.appendRecords([
    ["4", "original"],
    ["5", "original"],
    ["6", "original"]]);

  scrollContainer.delete(["1", "original"]);

  expect(scrollContainer.rows.length).toBe(5);
  expect(scrollContainer.rowChunks.length).toBe(2);
  expect(scrollContainer.rowChunks[0].getRow("1")).toBeUndefined();
  expect(scrollContainer.rowChunks[0].getRow("2")[0]).toBe("2");
  expect(scrollContainer.rowChunks[0].getRow("3")[0]).toBe("3");
  expect(scrollContainer.rowChunks[1].getRow("4")[0]).toBe("4");
  expect(scrollContainer.rowChunks[1].getRow("5")[0]).toBe("5");
  expect(scrollContainer.rowChunks[1].getRow("6")[0]).toBe("6");
});

test('Should insert a row', () => {
  const scrollContainer = new ScrollRowContainer(idGetter);
  scrollContainer.appendRecords([
    ["1", "original"],
    ["2", "original"],
    ["3", "original"]]);
  scrollContainer.appendRecords([
    ["4", "original"],
    ["5", "original"],
    ["6", "original"]]);

  scrollContainer.insert(2, ["2.5", "new"]);

  expect(scrollContainer.rows.length).toBe(7);
  expect(scrollContainer.rowChunks.length).toBe(2);
  expect(scrollContainer.rowChunks[0].getRow("1")[0]).toBe("1");
  expect(scrollContainer.rowChunks[0].getRow("2")[0]).toBe("2");
  expect(scrollContainer.rowChunks[0].getRow("2.5")[0]).toBe("2.5");
  expect(scrollContainer.rowChunks[0].getRow("3")[0]).toBe("3");
  expect(scrollContainer.rowChunks[1].getRow("4")[0]).toBe("4");
  expect(scrollContainer.rowChunks[1].getRow("5")[0]).toBe("5");
  expect(scrollContainer.rowChunks[1].getRow("6")[0]).toBe("6");
});

test('Should insert two rows', () => {
  const scrollContainer = new ScrollRowContainer(idGetter);
  scrollContainer.appendRecords([
    ["1", "original"],
    ["2", "original"],
    ["3", "original"]]);
  scrollContainer.appendRecords([
    ["4", "original"],
    ["5", "original"],
    ["6", "original"]]);

  scrollContainer.insert(0, ["0.5", "new"]);
  scrollContainer.insert(6, ["5.5", "new"]);

  expect(scrollContainer.rows.length).toBe(8);
  expect(scrollContainer.rowChunks.length).toBe(2);
  expect(scrollContainer.rowChunks[0].getRow("0.5")[0]).toBe("0.5");
  expect(scrollContainer.rowChunks[0].getRow("1")[0]).toBe("1");
  expect(scrollContainer.rowChunks[0].getRow("2")[0]).toBe("2");
  expect(scrollContainer.rowChunks[0].getRow("3")[0]).toBe("3");
  expect(scrollContainer.rowChunks[1].getRow("4")[0]).toBe("4");
  expect(scrollContainer.rowChunks[1].getRow("5")[0]).toBe("5");
  expect(scrollContainer.rowChunks[1].getRow("5.5")[0]).toBe("5.5");
  expect(scrollContainer.rowChunks[1].getRow("6")[0]).toBe("6");
});

test('Should insert row at the end', () => {
  const scrollContainer = new ScrollRowContainer(idGetter);
  scrollContainer.appendRecords([
    ["1", "original"],
    ["2", "original"],
    ["3", "original"]]);
  scrollContainer.appendRecords([
    ["4", "original"],
    ["5", "original"],
    ["6", "original"]]);

  scrollContainer.insert(6, ["7", "new"]);

  expect(scrollContainer.rows.length).toBe(7);
  expect(scrollContainer.rowChunks.length).toBe(2);
  expect(scrollContainer.rowChunks[0].getRow("1")[0]).toBe("1");
  expect(scrollContainer.rowChunks[0].getRow("2")[0]).toBe("2");
  expect(scrollContainer.rowChunks[0].getRow("3")[0]).toBe("3");
  expect(scrollContainer.rowChunks[1].getRow("4")[0]).toBe("4");
  expect(scrollContainer.rowChunks[1].getRow("5")[0]).toBe("5");
  expect(scrollContainer.rowChunks[1].getRow("6")[0]).toBe("6");
  expect(scrollContainer.rowChunks[1].getRow("7")[0]).toBe("7");
});
