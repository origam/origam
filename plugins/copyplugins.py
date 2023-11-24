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


def read_config(path_to_config):
    with open(path_to_config) as f:
        return json.load(f)


def main():
    path_to_config = Path("_copyplugins_config.json")
    config = read_config(path_to_config)
    origam_repo_path = Path(config['pathToOrigamRepo'])

    print(f"Choose plugin to copy to {origam_repo_path}")
    plugin_list = list(config["plugins"].keys())
    plugin_name = select_option(plugin_list, default=0)
    plugin_config = config["plugins"][plugin_name]

    print("plugin_name: " + plugin_name)

    source_package_json_path = Path(plugin_config["packageJsonPath"])
    target_package_json_path = Path(origam_repo_path) / "frontend-html" / "package.json"
    copy_dependencies(source_package_json_path, target_package_json_path)

    shutil.copy(
        plugin_config["registrationFilePath"],
        origam_repo_path / "frontend-html/src/plugins/tools/PluginRegistration.ts"
    )

    delete(origam_repo_path / "frontend-html/src/plugins/implementations")
    shutil.copytree(plugin_config["pluginSourcePath"],
              origam_repo_path / "frontend-html/src/plugins/implementations/plugins/src")

    shutil.copy(plugin_config["packageJsonPath"],
                origam_repo_path / "frontend-html/src/plugins/implementations/plugins")


if __name__ == "__main__":
    run_and_wait_for_key(main)
