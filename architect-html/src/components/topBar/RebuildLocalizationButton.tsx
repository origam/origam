import { RootStoreContext, T } from '@/main';
import Button from '@components/Button/Button';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { observer } from 'mobx-react-lite';
import { useContext, useState } from 'react';
import { VscGlobe } from 'react-icons/vsc';

const RebuildLocalizationButton = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const [isRunning, setIsRunning] = useState(false);
  const run = runInFlowWithHandler(rootStore.errorDialogController);

  const handleOnClick = () => {
    if (isRunning) return;
    run({
      action: async () => {
        setIsRunning(true);
        rootStore.progressBarState.isWorking = true;
        try {
          const files = await rootStore.architectApi.rebuildLocalizationFiles();
          rootStore.notificationState.pushActionResult({
            title: T('Localization files rebuilt', 'localization_rebuild_success'),
            subtitle: files.join(', '),
          });
        } finally {
          setIsRunning(false);
          rootStore.progressBarState.isWorking = false;
        }
      },
    });
  };

  if (!rootStore.packagesState.activePackageId) return null;

  return (
    <Button
      type="secondary"
      title={T('Rebuild Localization Files', 'localization_rebuild_label')}
      prefix={<VscGlobe />}
      isDisabled={isRunning}
      onClick={handleOnClick}
      dataTestId="topbar-rebuild-localization"
    />
  );
});

export default RebuildLocalizationButton;
