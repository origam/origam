export const NS = "ScreensActions";

export const START_SCREEN = `${NS}/START_SCREEN`;
export const STOP_SCREEN = `${NS}/STOP_SCREEN`;

export interface IStartScreen {
  NS: typeof NS,
  type: typeof START_SCREEN;
}
export const startScreen = (): IStartScreen => ({ NS, type: START_SCREEN });

export interface IStopScreen {
  NS: typeof NS,
  type: typeof STOP_SCREEN;
}
export const stopScreen = (): IStartScreen => ({ NS, type: STOP_SCREEN });
