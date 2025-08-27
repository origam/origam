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

import { Component } from '@editors/designerEditor/common/designerComponents/Component.tsx';
import SD from '@editors/designerEditor/common/DesignerSurface.module.scss';
import S from '@editors/designerEditor/common/SectionItem.module.scss';
import { observer } from 'mobx-react-lite';
import React from 'react';

export const SectionItem: React.FC<{
  components: Component[];
}> = observer(props => {
  return (
    <div className={SD.designSurface + S.sectionRoot}>
      {props.components.map(component => (
        <React.Fragment key={component.id}>
          <div
            className={SD.componentLabel}
            style={{
              ...component.getLabelStyle(),
              zIndex: component.zIndex,
            }}
          >
            {component.data.identifier}
          </div>
          <div
            className={SD.designComponent}
            style={{
              left: `${component.absoluteLeft}px`,
              top: `${component.absoluteTop}px`,
              width: `${component.width}px`,
              height: `${component.height}px`,
              zIndex: component.zIndex,
            }}
          >
            {component.designerRepresentation}
          </div>
        </React.Fragment>
      ))}
    </div>
  );
});
