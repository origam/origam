
import { registerPlugin } from "plugins/tools/PluginLibrary";
import { WeighingPlugin } from "plugins/implementations/WeighingPlugin/src";

export function registerPlugins() {
  registerPlugin("WeighingPlugin", () => new WeighingPlugin());
}
        