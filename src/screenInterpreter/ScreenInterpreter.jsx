function reactProcessNode(node: any, path: string[]) {
  if (path.length === 0) {
    // Root node
    return (
      <>
        {node.elements.map((element: any) =>
          reactProcessNode(element, [...path, element])
        )}
      </>
    );
  }
  switch (node.type) {
    case "FormRoot":
      break;
    case "Window": 
      break;
    case "Property":
      break;
    case "Properties":
      break;
    case "FormField":
      break;
    case "PropertyNames":
      break;
  }
}
