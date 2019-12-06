import { when } from "mobx";

export const delay = (ms: number) =>
  new Promise(resolve => setTimeout(resolve, ms));


export function* gWhen(pred: () => boolean) {
  if(!pred()) {
    yield when(pred)
  }
}