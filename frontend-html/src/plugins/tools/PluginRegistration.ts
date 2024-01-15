import { registerPlugin } from "plugins/tools/PluginLibrary";
import { WeighingPlugin } from "plugins/implementations/plugins/src";

export function registerPlugins() {
  registerPlugin("WeighingPlugin", () => new WeighingPlugin());
}