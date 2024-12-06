import { useContext, useEffect } from "react";
import { RootStoreContext } from "src/main.tsx";
import { observer } from "mobx-react-lite";
import { IPackage } from "src/API/IArchitectApi.ts";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";

export const Packages: React.FC = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const packagesState = rootStore.packagesState;

  useEffect(() => {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: packagesState.loadPackages.bind(packagesState),
    });
  }, []);

  return (
    <div>
      {packagesState.packages.map(x => <PackageItem key={x.id} package={x}/>)}
    </div>
  );
});

function PackageItem(props: {
  package: IPackage
}) {
  const rootStore = useContext(RootStoreContext);
  const packagesState = rootStore.packagesState;

  async function onPackageClick() {

    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: packagesState.setActivePackage(props.package.id)
    });
  }

  return (
    <div onClick={onPackageClick}>{props.package.name}</div>
  );
}


