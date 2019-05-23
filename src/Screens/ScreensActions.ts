export const NS = "ScreensActions";

export const START_SCREEN = `${NS}/START_SCREEN`;
export const STOP_SCREEN = `${NS}/STOP_SCREEN`;

export interface IStartScreen {
  type: typeof START_SCREEN;
}
export const startScreen = (): IStartScreen => ({ type: START_SCREEN });

export interface IStopScreen {
  type: typeof STOP_SCREEN;
}
export const stopScreen = (): IStartScreen => ({ type: STOP_SCREEN });
