import { T } from '@/main';
import DeploymentScriptsGeneratorButtonHOC from '@modules/deploymentScriptsGenerator/DeploymentScriptsGeneratorButtonHOC';
import ModelCheckButtonHOC from '@components/modelCheck/ModelCheckButtonHOC';
import RebuildLocalizationButton from '@components/topBar/RebuildLocalizationButton';
import S from '@components/topBar/ToolsMenu.module.scss';
import SettingsButtonHOC from '@modules/settings/SettingsButtonHOC';
import { useEffect, useRef } from 'react';
import { VscChevronDown, VscMenu } from 'react-icons/vsc';

const ToolsMenu = () => {
  const menuRef = useRef<HTMLDetailsElement>(null);

  useEffect(() => {
    const closeOnOutsideClick = (event: PointerEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        menuRef.current.open = false;
      }
    };
    document.addEventListener('pointerdown', closeOnOutsideClick);
    return () => document.removeEventListener('pointerdown', closeOnOutsideClick);
  }, []);

  return (
    <details
      ref={menuRef}
      className={S.root}
      onKeyDown={event => {
        if (event.key === 'Escape' && menuRef.current?.open) {
          menuRef.current.open = false;
          menuRef.current.querySelector('summary')?.focus();
        }
      }}
      onBlur={event => {
        if (!event.currentTarget.contains(event.relatedTarget as Node)) {
          event.currentTarget.open = false;
        }
      }}
    >
      <summary className={S.trigger} data-test-id="topbar-menu">
        <VscMenu />
        {T('Menu', 'topbar_menu_label')}
        <VscChevronDown />
      </summary>
      <div
        className={S.dropdown}
        onClick={event => {
          const button = (event.target as Element).closest('[data-test-id]');
          if (button && button.getAttribute('aria-disabled') !== 'true' && menuRef.current) {
            menuRef.current.open = false;
          }
        }}
      >
        <ModelCheckButtonHOC />
        <DeploymentScriptsGeneratorButtonHOC />
        <RebuildLocalizationButton />
        <SettingsButtonHOC />
      </div>
    </details>
  );
};

export default ToolsMenu;
