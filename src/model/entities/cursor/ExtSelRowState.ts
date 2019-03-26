import { IExtSelRowState } from "./types/ICursor";
import { observable } from "mobx";

export class ExtSelRowState implements IExtSelRowState{
  @observable selRowId: string | undefined;
}