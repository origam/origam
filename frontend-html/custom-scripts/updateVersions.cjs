const fs = require("fs");
const path = require('path');

const pluginRegistrationFilePath = "src/plugins/tools/PluginRegistration.ts"
const envFilePath = ".env.production.local";

const pluginVersionsVariableName = "VITE_REACT_APP_ORIGAM_UI_PLUGINS";

function getPackagePath(pluginName, registrationFile) {
  const pluginPackageRegEx = new RegExp('import\\s*{?\\s*[\\s,\\w]*\\s*' + pluginName + '\\s*[\\s,\\w]*\\s*}?\\s*from\\s*"([@\\w\\/\\-]+)', 'g');
  let packageMatch = pluginPackageRegEx.exec(registrationFile);
  if(!packageMatch){
    throw new Error(`Could not find import of plugin ${pluginName}`)
  }
  return packageMatch[1];
}

function getPackageNameAndVersionFromPackageJson(pathToPackageJson){
  const packageJson = JSON.parse(fs.readFileSync(pathToPackageJson).toString());
  return [packageJson["name"], packageJson["version"]];
}

function findUsedPackageNames(registrationFile) {
  const pluginRegEx = /registerPlugin\s*\(\s*"(\w+)",/g;
  return [...registrationFile.matchAll(pluginRegEx)].map(x => x[1]);
}

function findPackageJsonPath(importPath){

  let packageJsonPath = `src/${path.dirname(importPath)}/package.json`
  if(fs.existsSync(packageJsonPath)){
    return packageJsonPath;
  }
  packageJsonPath = `src/${path.dirname(path.dirname(importPath))}/package.json`
  if(fs.existsSync(packageJsonPath)){
    return packageJsonPath;
  }
  throw new Error(`Could not find package.json on path \"${importPath}\"`);
}

function setVariableValue(variableName, value, envFile) {
  if (envFile.includes(variableName)) {
    const regEx = new RegExp("(" + variableName + "\\s*=\\s*)(.*)", 'g');
    return envFile.replace(regEx, "$1" + value);
  } else {
    return `${envFile}\n${variableName} = ${value}`;
  }
}

if (!fs.existsSync(pluginRegistrationFilePath)) {
  throw new Error(
    `File '${pluginRegistrationFilePath}' was not found.`
  );
}

const registrationFile = fs.readFileSync(pluginRegistrationFilePath).toString();
const usedPluginNames = findUsedPackageNames(registrationFile)
const packageNamesAndVersions = usedPluginNames
  .map(pluginName => {
    const importPath = getPackagePath(pluginName, registrationFile);
    let packageVersion;
    let packageName;
    if(importPath.startsWith("plugins/implementations/")){
      const packageJsonPath = findPackageJsonPath(importPath);
      [packageName, packageVersion] = getPackageNameAndVersionFromPackageJson(packageJsonPath)
    }else{
      packageName = importPath;
      packageVersion = require(`../node_modules/${importPath}/package.json`).version;
    }
    return `${pluginName} - ${packageName}: ${packageVersion}`;
  })
  .join(";")


let newEnvFile;
if (fs.existsSync(envFilePath)) {
  const envFile = fs.readFileSync(envFilePath).toString();

  newEnvFile = setVariableValue(pluginVersionsVariableName, packageNamesAndVersions, envFile)
} else {
  newEnvFile = `${pluginVersionsVariableName} = ${packageNamesAndVersions}\n`;
}

fs.writeFileSync(envFilePath, newEnvFile);