import json
import os
import shutil
from pathlib import Path
from ui import select_option, run_and_wait_for_key
import _winapi


def link(source, destination):
    destination.parent.mkdir(parents=True, exist_ok=True)
    _winapi.CreateJunction(str(source), str(destination))


def add_dependencies(package_json_path, dependencies):
    with open(package_json_path, "r+") as f:
        content = f.read()
        for dependency in dependencies:
            if dependency not in content:
                content = content.replace('"dependencies": {', '"dependencies": {\n    ' + dependency + ",")
        f.seek(0)
        f.write(content)


def move_to_trash(path):
    try:
        send2trash(path)
    except PermissionError as ex:
        print(f"Cannot move ${ex.filename} to trash. Is Webstorm or something else running there?\n")
        raise Exception(f"Cannot move ${ex.filename} to trash. Is Webstorm or something else running there?", ex)


def delete(path):
    if not os.path.exists(path):
        return

    try:
        from send2trash import send2trash
        move_to_trash(path)
    except ModuleNotFoundError:
        print(f'Module "send2trash" was not found. So the path "{path}" will be deleted. '
              'If you prefer sending it to the trash install the module.')
        shutil.rmtree(path)


def copy_dependencies(source_package_json_path, target_package_json_path):
    with open(source_package_json_path) as f:
        source_package_json = json.load(f)
    dependency_dict = source_package_json["dependencies"]
    dependencies = [f'"{key}": "{dependency_dict[key]}"' for key in dependency_dict]
    add_dependencies(target_package_json_path, dependencies)
    print(f"Dependencies copied to: {target_package_json_path}")
    for dependency in dependencies:
        print(dependency)


def read_config(path_to_config):
    with open(path_to_config) as f:
        return json.load(f)


def main():
    path_to_config = Path("copyplugins_config.json")
    config = read_config(path_to_config)
    origam_repo_path = Path(config['pathToOrigamRepo'])
    frontend_path = origam_repo_path / "frontend-html"
    origam_plugin_src = origam_repo_path / "frontend-html/src/plugins/implementations/plugins/src"
    origam_plugin_root = origam_repo_path / "frontend-html/src/plugins/implementations/plugins"

    print("This script copies plugin sources between a plugin repository and origam client.")
    print("Choose a plugin:")
    plugin_list = list(config["plugins"].keys())
    plugin_name = select_option(plugin_list, default=0)
    plugin_config = config["plugins"][plugin_name]

    print("Dou you want to copy to or from the plugin repository?")
    copy_from_plugin_repo = "Copy from plugin repository"
    copy_to_plugin_repo = "Copy back to plugin repository"
    mode = select_option(list([copy_from_plugin_repo, copy_to_plugin_repo]), default=0)

    if mode == copy_from_plugin_repo:
        source_package_json_path = Path(plugin_config["packageJsonPath"])
        copy_dependencies(source_package_json_path, frontend_path / "package.json")

        shutil.copy(
            plugin_config["registrationFilePath"],
            origam_repo_path / "frontend-html/src/plugins/tools/PluginRegistration.ts"
        )
        print("Copied PluginRegistration.ts")

        delete(origam_repo_path / "frontend-html/src/plugins/implementations")
        shutil.copytree(plugin_config["pluginSourcePath"],
                        origam_plugin_src)
        print(f"Copied plugin sources to: {origam_plugin_src}")

        shutil.copy(plugin_config["packageJsonPath"],
                    origam_plugin_root)
        print(f"Copied plugin's package.json to: {origam_plugin_root}")

        if os.path.exists(plugin_config["yarnLockPath"]):
            shutil.copy(plugin_config["yarnLockPath"], frontend_path / "yarn.lock")
            print(f"Copied plugin's yarn.lock to: {frontend_path}")
        else:
            print(f"{plugin_config['yarnLockPath']} was not copied because it does not exist")
    else:
        delete(plugin_config["pluginSourcePath"])
        shutil.copytree(origam_plugin_src,
                        plugin_config["pluginSourcePath"])
        print(f"Copied plugin sources back to: {plugin_config['pluginSourcePath']}")

        shutil.copy(frontend_path / "yarn.lock", plugin_config["yarnLockPath"])
        print(f"Copied plugin's yarn.lock back to: {plugin_config['yarnLockPath']}")


if __name__ == "__main__":
    run_and_wait_for_key(main)
