import React, {useContext} from "react";
import {observer} from "mobx-react-lite";
import {MainMenu} from "./MainMenu";
import {MobXProviderContext} from "mobx-react";
import {IApplication} from "model/entities/types/IApplication";
import {getIsMainMenuLoading} from "model/selectors/MainMenu/getIsMainMenuLoading";
import {getMainMenu} from "../../../model/selectors/MainMenu/getMainMenu";
import {getWorkbenchLifecycle} from "../../../model/selectors/getWorkbenchLifecycle";

export const MainMenuPanel: React.FC = observer(props => {
  const application = useContext(MobXProviderContext)
    .application as IApplication;
  const isLoading = getIsMainMenuLoading(application);
  const mainMenu = getMainMenu(application);
  if (mainMenu && !isLoading) {
    return (
      <MainMenu
        menuUI={mainMenu.menuUI}
        onItemClick={(event: any, item: any) =>
          getWorkbenchLifecycle(application).onMainMenuItemClick({
            event: event,
            item: item,
            idParameter: undefined
          })
        }
      />
    );
  } else return null;
});
