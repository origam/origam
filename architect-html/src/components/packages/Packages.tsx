import { useContext, useEffect, useState } from "react";
import { ArchitectApiContext } from "src/API/ArchitectApiContext.tsx";
import { setActiveTab } from "src/components/tabView/TabViewSlice.ts";
import { useDispatch } from "react-redux";

export function Packages(props: {
  onPackageLoaded: ()=>void
}) {
  const architectApi = useContext(ArchitectApiContext)!;
  const [packages, setPackages] = useState<Package[]>([]);

  useEffect(() => {
    (async () => {
      setPackages(await architectApi.getPackages());
    })();
  }, []);

  return (
    <div>
      {packages.map(x => <PackageItem key={x.id} package={x} onPackageLoaded={props.onPackageLoaded}/>)}
    </div>
  );
}

function PackageItem(props: {
  package: Package,
  onPackageLoaded: ()=>void
}) {
  const dispatch = useDispatch();
  const architectApi = useContext(ArchitectApiContext)!;

  async function onPackageClick(){
    await architectApi.setActivePackage(props.package.id);
    props.onPackageLoaded();
    dispatch(setActiveTab({ instanceId: "SideBar", index: 1 }));
  }

  return (
      <div onClick={onPackageClick}>{props.package.name}</div>
  );
}


export interface Package {
  id: string
  name: string
}


