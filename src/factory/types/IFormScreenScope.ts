import {
  IFormScreen,
  IFormScreenMachine,
  IScreenContentFactory
} from "../../Screens/FormScreen/types";

export interface IFormScreenScope {
  formScreen: IFormScreen;
  screenMachine: IFormScreenMachine;
  screenContentFactory: IScreenContentFactory;
}
