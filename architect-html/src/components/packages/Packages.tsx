import { useContext, useEffect } from "react";
import { RootStoreContext } from "src/main.tsx";
import { observer } from "mobx-react-lite";
import { IPackage } from "src/API/IArchitectApi.ts";
import {
  runGeneratorInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";

export const Packages: React.FC = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const packagesState = rootStore.packagesState;

  useEffect(() => {
    runGeneratorInFlowWithHandler({
      controller: rootStore.errorDialogController,
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
  const modelTreeState = rootStore.modelTreeState;

  async function onPackageClick() {

    runGeneratorInFlowWithHandler({
      controller: rootStore.errorDialogController,
      generator: function* () {
        yield* packagesState.setActivePackage(props.package.id);
        rootStore.sideBarTabViewState.showModelTree();
        yield* modelTreeState.loadPackageNodes();
      },
    });
  }

  return (
    <div onClick={onPackageClick}>{props.package.name}</div>
  );
}


