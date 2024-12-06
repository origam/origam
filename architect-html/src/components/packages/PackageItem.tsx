import { IPackage } from "src/API/IArchitectApi.ts";
import { useContext } from "react";
import { RootStoreContext } from "src/main.tsx";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import S from "src/components/packages/PackageItem.module.scss"

export function PackageItem(props: {
  package: IPackage,
  isSelected?: boolean
}) {
  const rootStore = useContext(RootStoreContext);
  const packagesState = rootStore.packagesState;

  async function onPackageClick() {

    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: packagesState.setActivePackage(props.package.id)
    });
  }

  return (
    <div className={S.root + " " + (props.isSelected ? S.selected : "")} onClick={onPackageClick}>{props.package.name}</div>
  );
}