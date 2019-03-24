import { IViewType } from "src/presenter/types/IScreenPresenter";

export function parseBoolean(value: string | undefined) {
  return value === "true";
}

export function parseNumber(value: string | undefined) {
  return value !== undefined ? parseInt(value, 10) : undefined;
}

export function parseViewType(value: string) {
  switch (value) {
    default:
    case "0":
      return IViewType.FormView;
    case "1":
      return IViewType.TableView;
  }
}
