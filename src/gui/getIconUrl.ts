export function getIconUrl(iconName: string) {
  switch (iconName) {
    case "menu_form.png":
      return "./icons/document.svg";
    case "menu_workflow.png":
      return "./icons/settings.svg";
    case "./icons/work-queue.svg": 
      return "./icons/work-queue.svg";
    default:
      return "./icons/document.svg";
  }
}