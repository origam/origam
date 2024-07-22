import React, { useEffect, useState } from "react";
import axios from "axios";

export function Packages(props: {
  onPackageLoaded: ()=>void
}) {
  const [packages, setPackages] = useState<Package[]>([]);

  useEffect(() => {
    (async () => {
      setPackages((await axios.get("/Package/GetAll")).data);
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

  async function onPackageClick(){
    await axios.post("/Package/SetActive", {id: props.package.id});
    props.onPackageLoaded();
  }

  return (
      <div onClick={onPackageClick}>{props.package.name}</div>
  );
}


export interface Package {
  id: string
  name: string
}