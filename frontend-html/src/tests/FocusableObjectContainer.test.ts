import { FocusAbleObjectContainer, IFocusAble } from "../model/entities/FocusManager";

class DummyFocusable implements IFocusAble{
  disabled: boolean = false;
  tabIndex: number = 0;
  focus(): void {
  }
}

test.each([
  [["1", "5"], -1],
  [["5", "5"], 0],
  [["10", "5"], 1],
  [["1.12 ", "1.15"], -1],
  [["1.12", "1.2"], 1],
  [["1.12", "1.12"], 0],
  [["5", "5.2"], -1],
  [["5.12.14", "5.12.5"], 1],
  [["5.12.5", "5.12.15"], -1],
  [["5.12.5", "5.12.5"], 0],
  [["5", "5.12.5"], -1],
  [[undefined, undefined], 0],
  [[undefined, "1"], 1],
  [["1", undefined], -1],
])('Compare %s to: %s', (values, expectedNormalizedResult) => {
  const val1 = new FocusAbleObjectContainer(new DummyFocusable(), "", values[0])
  const val2 = new FocusAbleObjectContainer(new DummyFocusable(), "", values[1])
  const comparisonResult = FocusAbleObjectContainer.compare(val1, val2)

  const sign = Math.sign(comparisonResult);
  const normalizedResult = sign != 0
    ? comparisonResult / comparisonResult * sign
    : comparisonResult;

  expect(normalizedResult).toBe(expectedNormalizedResult);
});