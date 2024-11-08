import React, { useState, useRef } from 'react';
import { useDrop } from 'react-dnd';
import S from './DesignSurface.module.scss';
import {
  ILabelItem
} from "src/components/screenSectionEditor2/designComponents/ILabelItem.ts";
import {
  DraggableLabel
} from "src/components/screenSectionEditor2/designComponents/DraggableLabel.tsx";


export const DesignSurface: React.FC = () => {
  const [labels, setLabels] = useState<ILabelItem[]>([]);
  const surfaceRef = useRef<HTMLDivElement>(null);

  const [, drop] = useDrop(() => ({
    accept: 'LABEL',
    drop: (item: { text: string, id?: string }, monitor) => {
      const offset = monitor.getSourceClientOffset();
      const surfaceRect = surfaceRef.current?.getBoundingClientRect();

      if (offset && surfaceRect) {
        const x = offset.x - surfaceRect.left;
        const y = offset.y - surfaceRect.top;

        if (item.id) {
          setLabels((prevLabels) => {
            const existingLabel = prevLabels.find(label => label.id === item.id);
            if (!existingLabel) {
              throw new Error("Label with id " + item.id + " not found");
            }
            existingLabel.position = {x, y};
            return [...prevLabels]
          });
        } else {
          const newLabel: ILabelItem = {
            id: `${item.text}-${Date.now()}`, // Generate a unique id
            text: item.text,
            position: {x, y},
          };
          setLabels((prevLabels) => [...prevLabels, newLabel]);
        }
      }
    },
  }));

  return (
    <div ref={drop} className={S.root}>
      <div ref={surfaceRef}>
        {labels.map((label) => (
          <div
            key={label.id}
            style={{
              position: 'absolute',
              left: `${label.position.x}px`,
              top: `${label.position.y}px`,
            }}
          >
            <DraggableLabel text={label.text} id={label.id}/>
          </div>
        ))}
      </div>
    </div>
  );
};