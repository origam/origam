import { observable } from "mobx";
import { GroupItem } from "../GroupItem";

export function rootGroups() {
  return observable([
    new GroupItem([], [], "Column 1", "Value 1"),
    new GroupItem(
      [
        new GroupItem(
          [],
          [
            [1, 2, 3, "A", "B", "C"],
            [4, 5, 6, "D", "E", "F"],
            [7, 8, 9, "G", "H", "I"],
            [10, 11, 12, "J", "K", "L"],
          ],
          "Column 2",
          "Value 1"
        ),
        new GroupItem([], [], "Column 2", "Value 2"),
        new GroupItem([], [], "Column 2", "Value 3"),
        new GroupItem(
          [],
          [
            [1, 2, 3, "A", "B", "C"],
            [4, 5, 6, "D", "E", "F"],
            [7, 8, 9, "G", "H", "I"],
            [10, 11, 12, "J", "K", "L"],
          ],
          "Column 2",
          "Value 4"
        ),
        new GroupItem([], [], "Column 2", "Value 5"),
      ],
      [],
      "Column 1",
      "Value 2"
    ),
    new GroupItem([], [], "Column 1", "Value 3"),
    new GroupItem([], [], "Column 1", "Value 4"),
    new GroupItem([], [], "Column 1", "Value 5"),
  ]);
}
