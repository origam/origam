/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o. 

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { RootStoreContext, T } from '@/main';
import { ITabViewItem, TabView } from '@components/tabView/TabView';
import S from '@editors/designerEditor/common/Toolbox.module.scss';
import { ToolboxState } from '@editors/designerEditor/common/ToolboxState';
import { FilterableSelect } from '@editors/propertyEditor/FilterableSelect';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { observer } from 'mobx-react-lite';
import { useContext, useMemo, useRef } from 'react';

const MIN_DESIGN_SURFACE_WIDTH = 200;

const Toolbox = observer(
  ({
    toolboxState,
    tabViewItems,
  }: {
    toolboxState: ToolboxState;
    tabViewItems: ITabViewItem[];
  }) => {
    const rootStore = useContext(RootStoreContext);
    const run = runInFlowWithHandler(rootStore.errorDialogController);
    const toolboxRef = useRef<HTMLDivElement>(null);
    const resizeStartRef = useRef<{ pointerX: number; width: number; maxWidth: number } | null>(
      null,
    );

    const startResizing = (event: React.PointerEvent<HTMLDivElement>) => {
      const toolbox = toolboxRef.current;
      if (!toolbox?.parentElement) {
        return;
      }
      event.preventDefault();
      (document.activeElement as HTMLElement | null)?.blur();
      event.currentTarget.setPointerCapture(event.pointerId);
      resizeStartRef.current = {
        pointerX: event.clientX,
        width: toolbox.offsetWidth,
        maxWidth: toolbox.parentElement.clientWidth - MIN_DESIGN_SURFACE_WIDTH,
      };
      document.body.style.cursor = 'col-resize';
      document.body.style.userSelect = 'none';
    };

    const resize = (event: React.PointerEvent<HTMLDivElement>) => {
      const resizeStart = resizeStartRef.current;
      if (!resizeStart) {
        return;
      }
      rootStore.uiState.setDesignerToolboxWidth(
        Math.min(resizeStart.maxWidth, resizeStart.width + event.clientX - resizeStart.pointerX),
      );
    };

    const stopResizing = () => {
      resizeStartRef.current = null;
      document.body.style.cursor = '';
      document.body.style.userSelect = '';
    };

    const dataSourceOptions = useMemo(
      () =>
        toolboxState.dataSources.map(dataSource => ({
          value: dataSource.schemaItemId,
          name: dataSource.name,
        })),
      [toolboxState.dataSources],
    );

    return (
      <div
        ref={toolboxRef}
        data-test-id="toolbox"
        className={S.toolbox}
        style={{
          width: `min(${rootStore.uiState.designerToolboxWidth}px, calc(100% - ${MIN_DESIGN_SURFACE_WIDTH}px))`,
        }}
      >
        <div className={S.inputs}>
          <div className={S.inputContainer}>
            <div>{T('Data Source', 'tool_box_data_source')}</div>
            <FilterableSelect
              className={S.dataSourceSelect}
              options={dataSourceOptions}
              selectedValue={toolboxState.selectedDataSourceId ?? ''}
              onChange={value =>
                run({ generator: toolboxState.selectedDataSourceIdChanged(value) })
              }
            />
          </div>
          <div className={S.inputContainer}>
            <div>{T('Name', 'tool_box_name')}</div>
            <input
              type="text"
              value={toolboxState.name}
              onChange={e =>
                run({
                  generator: toolboxState.nameChanged(e.target.value.replace(/\s+/gu, '')),
                })
              }
            />
          </div>
          <div className={S.inputContainer}>
            <div>Id</div>
            <input disabled={true} type="text" value={toolboxState.id} />
          </div>
          <div className={S.inputContainer}>
            <div>{T('Package', 'tool_box_package')}</div>
            <input disabled={true} type="text" value={toolboxState.schemaExtensionId} />
          </div>
        </div>
        <TabView state={toolboxState.tabViewState} items={tabViewItems} />
        <div
          className={S.splitter}
          role="separator"
          aria-orientation="vertical"
          onPointerDown={startResizing}
          onPointerMove={resize}
          onLostPointerCapture={stopResizing}
        />
      </div>
    );
  },
);

export default Toolbox;
