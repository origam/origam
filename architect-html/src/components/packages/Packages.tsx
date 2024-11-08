import { useContext, useEffect } from "react";
import { RootStoreContext } from "src/main.tsx";
import { observer } from "mobx-react-lite";
import { flow } from "mobx";
import { IPackage } from "src/API/IArchitectApi.ts";

export const Packages: React.FC = observer(() => {
  const projectState = useContext(RootStoreContext).projectState;

  useEffect(() => {
    flow(projectState.loadPackages.bind(projectState))();
  }, []);

  return (
    <div>
      {projectState.packages.map(x => <PackageItem key={x.id} package={x}/>)}
    </div>
  );
});

function PackageItem(props: {
  package: IPackage
}) {
  const projectState = useContext(RootStoreContext).projectState;

  async function onPackageClick() {
    flow(function* () {
      yield* projectState.setActivePackage(props.package.id);
      projectState.showModelTree();
      yield* projectState.loadPackageNodes();
    })();
  }

  return (
    <div onClick={onPackageClick}>{props.package.name}</div>
  );
}


