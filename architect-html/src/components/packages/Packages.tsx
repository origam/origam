import { useContext, useEffect } from "react";
import { RootStoreContext } from "src/main.tsx";
import { observer } from "mobx-react-lite";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import { PackageItem } from "src/components/packages/PackageItem.tsx";
import S from "src/components/packages/Packages.module.scss"

export const Packages: React.FC = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const packagesState = rootStore.packagesState;

  useEffect(() => {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: packagesState.loadPackages.bind(packagesState),
    });
  }, []);

  return (
    <div className={S.root}>
      {packagesState.packages.map(x =>
        <PackageItem
          key={x.id}
          isSelected={packagesState.activePackageId === x.id}
          package={x}
        />)}
    </div>
  );
});


