import { registerPlugin } from "plugins/tools/PluginLibrary";
import { WeighingPlugin } from "plugins/implementations/DeponiePlugin/src";

export function registerPlugins() {
  registerPlugin("WeighingPlugin", () => new WeighingPlugin());
}