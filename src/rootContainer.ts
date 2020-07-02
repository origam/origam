import {Container, ILifetime} from "dic/Container";
import {registerModules} from "registerModules";

const $root = new Container({defaultLifetime: ILifetime.PerLifetimeScope});
registerModules($root);

export default $root;