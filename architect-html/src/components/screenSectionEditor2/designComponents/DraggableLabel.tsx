import { useDrag } from "react-dnd";
import {
  Label
} from "src/components/screenSectionEditor2/designComponents/Label.tsx";

export const DraggableLabel = (props: {
  text: string;
  id?: string;
}) => {
  const [{isDragging}, drag] = useDrag(() => ({
    type: 'LABEL',
    item: {text: props.text, id: props.id},
    collect: (monitor) => ({
      isDragging: !!monitor.isDragging(),
    }),
  }));

  return (
    <div ref={drag} style={{opacity: isDragging ? 0.5 : 1}}>
      <Label text={props.text}/>
    </div>
  );
};