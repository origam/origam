import json
import os
import re
import shutil
from pathlib import Path
from ui import select_option, run_and_wait_for_key

import sys

path_to_home = Path(sys.argv[0]).parent
path_to_config = path_to_home / Path("pluginmanager_config.json")
origam_repo_path = Path()
frontend_path = Path()
origam_plugin_folder = Path()
config = None


class PluginConfig:
    def __init__(self, name, plugin_source_path, package_json_path, registration_file_path, yarn_lock_path):
        self.name = name
        self.plugin_source_path = plugin_source_path
        self.package_json_path = package_json_path
        self.registration_file_path = registration_file_path
        self.yarn_lock_path = yarn_lock_path


def add_dependencies(package_json_path, dependencies):
    with open(package_json_path, "r+") as f:
        content = f.read()
        for dependency in dependencies:
            if dependency not in content:
                content = re.sub(r'[^\S\r\n]*"dependencies"\s*:\s*{', f'  "dependencies": {{\n    {dependency},',
                                 content)
        f.seek(0)
        f.write(content)


def copy_dependencies(source_package_json_path, target_package_json_path):
    with open(source_package_json_path) as f:
        source_package_json = json.load(f)
    if "dependencies" not in source_package_json:
        print(f"No dependencies found in: {target_package_json_path}")
        return
    dependency_dict = source_package_json["dependencies"]
    dependencies = [f'"{key}": "{dependency_dict[key]}"' for key in dependency_dict]
    add_dependencies(target_package_json_path, dependencies)
    print(f"Dependencies copied to: {target_package_json_path}")
    for dependency in dependencies:
        print(dependency)


def read_config(path_to_config):
    if not os.path.exists(path_to_config):
        raise Exception(f"Config {path_to_config} not found")
    with open(path_to_config) as f:
        config_content = json.load(f)

    if "pathToOrigamRepo" not in config_content or not config_content["pathToOrigamRepo"]:
        raise Exception(f"Config must contain \"pathToOrigamRepo\"")
    if "plugins" not in config_content:
        raise Exception(f"Config must contain \"plugins\" node")
    return config_content


def save_config(config_dict, path_to_config):
    with open(path_to_config, "w") as config_file:
        config_file.write(json.dumps(config_dict, indent=4))


def copy_from_plugin(plugin_config: PluginConfig):
    source_package_json_path = Path(plugin_config.package_json_path)
    copy_dependencies(source_package_json_path, frontend_path / "package.json")
    shutil.copy(
        plugin_config.registration_file_path,
        origam_repo_path / "frontend-html/src/plugins/tools/PluginRegistration.ts"
    )
    print("Copied PluginRegistration.ts")
    origam_plugin_src = get_origam_plugin_src(plugin_config)
    origam_plugin_root = get_origam_plugin_root(plugin_config)
    shutil.rmtree(origam_repo_path / "frontend-html/src/plugins/implementations")
    shutil.copytree(plugin_config.plugin_source_path,
                    origam_plugin_src)
    print(f"Copied plugin sources to: {origam_plugin_src}")
    shutil.copy(plugin_config.package_json_path,
                origam_plugin_root)
    print(f"Copied plugin's package.json to: {origam_plugin_root}")
    if os.path.exists(plugin_config.yarn_lock_path):
        shutil.copy(plugin_config.yarn_lock_path, frontend_path / "yarn.lock")
        print(f"Copied plugin's yarn.lock to: {frontend_path}")
    else:
        print(f"{plugin_config.yarn_lock_path} was not copied because it does not exist")


def copy_to_plugin(plugin_config: PluginConfig):
    shutil.rmtree(plugin_config.plugin_source_path)
    shutil.copytree(get_origam_plugin_src(plugin_config),
                    plugin_config.plugin_source_path)
    print(f"Copied plugin sources back to: {plugin_config.plugin_source_path}")
    shutil.copy(frontend_path / "yarn.lock", plugin_config.yarn_lock_path)
    print(f"Copied plugin's yarn.lock back to: {plugin_config.yarn_lock_path}")
    shutil.copy(
        origam_repo_path / "frontend-html/src/plugins/tools/PluginRegistration.ts",
        plugin_config.registration_file_path,
    )
    print(f"Copied PluginRegistration.ts back to: {plugin_config.registration_file_path}")


def init_new_plugin():
    plugin_base_name = input('Plugin name in PascalCase (for example: "Chart", "CustomerView"...):').strip()
    if not plugin_base_name:
        print("Need a plugin name")
        return
    plugin_name = plugin_base_name + "Plugin"

    parent_folder = Path(input("Where should we create the plugin folder:").strip())
    if not parent_folder.is_dir():
        print("That is not a path to a folder")
        return
    plugin_folder = parent_folder / plugin_name
    os.mkdir(plugin_folder)

    package_json_contents = {
        "name": f"@origam/{plugin_name}",
        "version": "1.0.0",
        "dependencies": {},
    }

    with open(plugin_folder / "package.json", "w") as package_json_file:
        package_json_file.write(json.dumps(package_json_contents, indent=4))

    registration_contents = f'''import {{ {plugin_name} }} from "plugins/implementations/plugins/src/{plugin_name}";
import {{ registerPlugin }} from "plugins/tools/PluginLibrary";


export function registerPlugins() {{
  registerPlugin("{plugin_name}", () => new {plugin_name}());
}}
'''
    with open(plugin_folder / "PluginRegistration.ts", "w") as registration_file:
        registration_file.write(registration_contents)

    src_folder = plugin_folder / "src"
    os.mkdir(src_folder)

    plugin_contents = f'''import React from "react";
import {{ observer }} from "mobx-react";
import {{ observable }} from "mobx";
import S from "./{plugin_name}.module.scss";
import {{ ILocalization, ILocalizer, ISectionPlugin, ISectionPluginData }} from "@origam/plugins";

export class {plugin_name} implements ISectionPlugin {{
  $type_ISectionPlugin: 1 = 1;
  id: string = "{plugin_name}";

  getScreenParameters: (() => {{ [p: string]: string }}) | undefined;

  getComponent(data: ISectionPluginData, createLocalizer: (localizations: ILocalization[]) => ILocalizer): React.ReactElement {{
    return <{plugin_base_name}Component
      pluginData={{data}}
    />;
  }}

  onSessionRefreshed() {{
  }}

  initialize(xmlAttributes: {{ [key: string]: string }}): void {{
  }}
}}


@observer
export class {plugin_base_name}Component extends React.Component<{{
  pluginData: ISectionPluginData,
}}> {{

  render(){{
    return <div className={{S.root}}>
      This is {plugin_name}
    </div>
  }}
}}
'''

    with open(src_folder / f"{plugin_name}.tsx", "w") as plugin_file:
        plugin_file.write(plugin_contents)

    with open(src_folder / f"{plugin_name}.module.scss", "w") as plugin_file:
        plugin_file.write(".root {}")

    plugin_config = {
        "pluginSourcePath": str(plugin_folder / "src"),
        "packageJsonPath": str(plugin_folder / "package.json"),
        "registrationFilePath": str(plugin_folder / "PluginRegistration.ts"),
        "yarnLockPath": str(plugin_folder / "yarn.lock")
    }
    config["plugins"][plugin_name] = plugin_config
    save_config(config, path_to_config)


def ask_for_plugin_config(config) -> PluginConfig:
    print("Choose a plugin:")
    plugin_list = list(config["plugins"].keys())
    plugin_name = select_option(plugin_list, default=0)
    plugin_config_dict = config["plugins"][plugin_name]
    return PluginConfig(
        name=plugin_name,
        plugin_source_path=plugin_config_dict["pluginSourcePath"],
        package_json_path=plugin_config_dict["packageJsonPath"],
        registration_file_path=plugin_config_dict["registrationFilePath"],
        yarn_lock_path=plugin_config_dict["yarnLockPath"])


def get_origam_plugin_src(plugin_config: PluginConfig):
    return origam_plugin_folder / plugin_config.name / "src"


def get_origam_plugin_root(plugin_config: PluginConfig):
    return origam_plugin_folder / plugin_config.name


def main():
    global config
    config = read_config(path_to_config)
    global origam_repo_path
    origam_repo_path = Path(config['pathToOrigamRepo'])
    global frontend_path
    frontend_path = origam_repo_path / "frontend-html"
    global origam_plugin_folder
    origam_plugin_folder = origam_repo_path / "frontend-html/src/plugins/implementations"

    print("This script will help you manage Origam front end plugins.")
    print("What do you want to do? Type an option number and hit Enter.")
    crete_new_plugin = "Create new plugin"
    copy_from_plugin_repo = "Copy from plugin repository to Origam repository"
    copy_to_plugin_repo = "Copy back to plugin repository from Origam repository"
    options = [crete_new_plugin]
    if config["plugins"] and len(config["plugins"].keys()) > 0:
        options.append(copy_from_plugin_repo)
        options.append(copy_to_plugin_repo)
    task = select_option(options, default=0)

    if task == copy_from_plugin_repo:
        plugin_config = ask_for_plugin_config(config)
        copy_from_plugin(plugin_config)
    elif task == copy_to_plugin_repo:
        plugin_config = ask_for_plugin_config(config)
        copy_to_plugin(plugin_config)
    elif task == crete_new_plugin:
        init_new_plugin()
    else:
        raise Exception(f"{task} not implemented")


if __name__ == "__main__":
    run_and_wait_for_key(main)
