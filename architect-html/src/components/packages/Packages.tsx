import { useContext, useEffect } from "react";
import { RootStoreContext } from "src/main.tsx";
import { observer } from "mobx-react-lite";
import { flow } from "mobx";
import { IPackage } from "src/API/IArchitectApi.ts";

export const Packages: React.FC = observer(() => {
  const packagesState = useContext(RootStoreContext).projectState.packagesState;

  useEffect(() => {
    flow(packagesState.loadPackages.bind(packagesState))();
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
  const projectState = useContext(RootStoreContext).projectState;
  const packagesState = useContext(RootStoreContext).projectState.packagesState;

  async function onPackageClick() {
    flow(function* () {
      yield* packagesState.setActivePackage(props.package.id);
      projectState.showModelTree();
      yield* projectState.loadPackageNodes();
    })();
  }

  return (
    <div onClick={onPackageClick}>{props.package.name}</div>
  );
}


